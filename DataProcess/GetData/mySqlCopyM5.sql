DROP PROCEDURE copy_m5data;

CREATE PROCEDURE copy_m5data(
     IN  stock_cd VARCHAR(6)       -- 代码
    ,IN  start_dt VARCHAR(14)      -- 开始时间
    ,IN  db_name VARCHAR(10)       -- 要导入的数据表的名称
    ,IN  sum_dt VARCHAR(100)       -- 可以合计的时间点
    )

BEGIN
  DECLARE openVal decimal(7,3) DEFAULT 0.0;
  DECLARE closeVal decimal(7,3) DEFAULT 0.0;
  DECLARE maxVal decimal(7,3) DEFAULT 0.0;
  DECLARE minVal decimal(7,3) DEFAULT 0.0;
  DECLARE openTmp decimal(7,3) DEFAULT 0.0;
  DECLARE maxValTmp decimal(7,3) DEFAULT 0.0;
  DECLARE minValTmp decimal(7,3) DEFAULT 9999.0;
  DECLARE curDt DATETIME;
  DECLARE curTime VARCHAR(6);
  DECLARE startFlg INT DEFAULT 0;
  
  DECLARE SQL_FOR_INSERT varchar(500);
  DECLARE done INT DEFAULT FALSE;
  
  -- 关闭事务自动提交
  -- SET autocommit=0;

  -- 查询原始数据
  DECLARE curM5 CURSOR FOR 
      SELECT 
           t.datetime
          ,date_format(t.datetime, '%H%i%s') AS curTime
          ,t.open_val
          ,t.close_val
          ,t.min_val
          ,t.max_val
      FROM data_m5 t
      WHERE t.code = stock_cd
        AND t.datetime >= str_to_date(start_dt, '%Y%m%d%H%i%s');
  
  DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
  
  -- 打开游标
  OPEN curM5;
  -- 开始循环
  read_loop: LOOP
    -- 提取游标里的数据
    FETCH curM5 INTO curDt, curTime, openVal, closeVal, minVal, maxVal;
    
    -- 声明结束的时候
    IF done THEN
      LEAVE read_loop;
    END IF;
    
    -- 开始计算最大值，最新值
    IF maxVal > maxValTmp THEN
        set maxValTmp = maxVal;
    END IF;
    
    IF minVal < minValTmp THEN
        set minValTmp = minVal;
    END IF;
    
    IF locate(curTime, sum_dt) > 0 THEN
        -- 生成插入sql语句
        set SQL_FOR_INSERT = CONCAT("insert into ", db_name
           , " (code,datetime,open_val,close_val,min_val,max_val) VALUES ('"
           , stock_cd, "','", curDt, "',", openTmp, ",", closeVal, ",", minValTmp, ",", maxValTmp, ");" );
        set @sql = SQL_FOR_INSERT;
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        deallocate prepare stmt; -- 释放prepare
        
        set startFlg = 0;
        
    ELSEIF startFlg = 0 THEN
        -- 重新开始，设置默认值
        set openTmp = openVal;
        set minValTmp = minVal;
        set maxValTmp = maxVal;
        
        set startFlg = 1;
        
    END IF;
    
  END LOOP;
  CLOSE curM5;
  
END;

