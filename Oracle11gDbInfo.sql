--STOCK
CREATE TABLESPACE STOCK_DATA
LOGGING
DATAFILE 'D:\app\OracleData\STOCK.DBF' 
SIZE 4096M 
AUTOEXTEND ON
NEXT 20M MAXSIZE 16384M
EXTENT MANAGEMENT LOCAL 
SEGMENT SPACE MANAGEMENT AUTO;

CREATE TABLESPACE STOCK_IDX
LOGGING
DATAFILE 'D:\app\OracleData\STOCK_IDX.DBF' 
SIZE 1024M 
AUTOEXTEND ON
NEXT 20M MAXSIZE 4000M
EXTENT MANAGEMENT LOCAL
SEGMENT SPACE MANAGEMENT AUTO;

--STOCK
CREATE TABLESPACE STOCK_DATA2
LOGGING
DATAFILE 'D:\app\OracleData\STOCK2.DBF' 
SIZE 8G 
AUTOEXTEND ON
NEXT 20M MAXSIZE 12G
EXTENT MANAGEMENT LOCAL 
SEGMENT SPACE MANAGEMENT AUTO;

CREATE TABLESPACE STOCK_IDX2
LOGGING
DATAFILE 'D:\app\OracleData\STOCK_IDX2.DBF' 
SIZE 3G 
AUTOEXTEND ON
NEXT 20M MAXSIZE 7G
EXTENT MANAGEMENT LOCAL
SEGMENT SPACE MANAGEMENT AUTO;

CREATE USER xjsxjsOra IDENTIFIED BY Xayr!234
DEFAULT TABLESPACE STOCK_DATA
TEMPORARY TABLESPACE TEMP;

GRANT CONNECT, RESOURCE, DBA TO xjsxjsOra;

--GRANT SYSDBA TO xjsxjsOra;


-- 创建M5表（5分钟粒度数据）
CREATE TABLE M5 (
    trade_date     DATE,           -- 交易日期时间（精确到5分钟）
    stock_code     VARCHAR2(6),    -- 证券代码（6位字符）
    stock_name     VARCHAR2(50),   -- 证券名称（最长50字符）
    close_price    NUMBER(8,3),    -- 收盘价（格式：99999.999）
    high_price     NUMBER(8,3),    -- 周期最高价
    low_price      NUMBER(8,3),    -- 周期最低价
    open_price     NUMBER(8,3),    -- 开盘价
    -- 主键定义（索引自动创建到STOCK_IDX表空间）
    CONSTRAINT pk_m5 PRIMARY KEY (stock_code)
    USING INDEX TABLESPACE STOCK_IDX
)
TABLESPACE STOCK_DATA;  -- 替换为实际数据表空间名

-- 创建日期字段索引到STOCK_IDX表空间
CREATE INDEX idx_m5_tradedate ON M5(trade_date)
TABLESPACE STOCK_IDX;

-- 添加表注释
COMMENT ON TABLE M5 IS '5分钟粒度证券行情数据表';

-- 添加字段注释
COMMENT ON COLUMN M5.trade_date  IS '数据时间戳（精确到5分钟）';
COMMENT ON COLUMN M5.stock_code  IS '证券唯一代码（如：600000）';
COMMENT ON COLUMN M5.stock_name  IS '证券完整名称（如：浦发银行）';
COMMENT ON COLUMN M5.close_price IS '时段收盘价（示例：12345.678）';
COMMENT ON COLUMN M5.high_price  IS '时段最高成交价';
COMMENT ON COLUMN M5.low_price   IS '时段最低成交价';
COMMENT ON COLUMN M5.open_price  IS '时段开盘价';

-- 创建M30表（30分钟粒度数据）
CREATE TABLE M30 (
    trade_date     DATE,           -- 交易日期时间（精确到30分钟）
    stock_code     VARCHAR2(6),    -- 证券代码（6位字符）
    stock_name     VARCHAR2(50),   -- 证券名称（最长50字符）
    close_price    NUMBER(8,3),    -- 收盘价（格式：99999.999）
    high_price     NUMBER(8,3),    -- 周期最高价
    low_price      NUMBER(8,3),    -- 周期最低价
    open_price     NUMBER(8,3),    -- 开盘价
    -- 主键定义（索引自动创建到STOCK_IDX表空间）
    CONSTRAINT pk_m30 PRIMARY KEY (stock_code)
    USING INDEX TABLESPACE STOCK_IDX2
)
TABLESPACE STOCK_DATA2;  -- 替换为实际数据表空间名

-- 创建日期字段索引到STOCK_IDX表空间
CREATE INDEX idx_m30_tradedate ON M30(trade_date)
TABLESPACE STOCK_IDX2;

-- 添加表注释
COMMENT ON TABLE M30 IS '30分钟粒度证券行情数据表';

-- 添加字段注释
COMMENT ON COLUMN M30.trade_date  IS '数据时间戳（精确到30分钟）';
COMMENT ON COLUMN M30.stock_code  IS '证券唯一代码（如：600000）';
COMMENT ON COLUMN M30.stock_name  IS '证券完整名称（如：浦发银行）';
COMMENT ON COLUMN M30.close_price IS '时段收盘价（示例：12345.678）';
COMMENT ON COLUMN M30.high_price  IS '时段最高成交价';
COMMENT ON COLUMN M30.low_price   IS '时段最低成交价';
COMMENT ON COLUMN M30.open_price  IS '时段开盘价';

-- 创建Day表（天粒度数据）
CREATE TABLE Day (
    trade_date     DATE,           -- 交易日期时间（精确到天）
    stock_code     VARCHAR2(6),    -- 证券代码（6位字符）
    stock_name     VARCHAR2(50),   -- 证券名称（最长50字符）
    close_price    NUMBER(8,3),    -- 收盘价（格式：99999.999）
    high_price     NUMBER(8,3),    -- 周期最高价
    low_price      NUMBER(8,3),    -- 周期最低价
    open_price     NUMBER(8,3),    -- 开盘价
    -- 主键定义（索引自动创建到STOCK_IDX表空间）
    CONSTRAINT pk_Day PRIMARY KEY (stock_code)
    USING INDEX TABLESPACE STOCK_IDX2
)
TABLESPACE STOCK_DATA2;  -- 替换为实际数据表空间名

-- 创建日期字段索引到STOCK_IDX表空间
CREATE INDEX idx_Day_tradedate ON Day(trade_date)
TABLESPACE STOCK_IDX2;

-- 添加表注释
COMMENT ON TABLE Day IS '天粒度证券行情数据表';

-- 添加字段注释
COMMENT ON COLUMN Day.trade_date  IS '数据时间戳（精确到天）';
COMMENT ON COLUMN Day.stock_code  IS '证券唯一代码（如：600000）';
COMMENT ON COLUMN Day.stock_name  IS '证券完整名称（如：浦发银行）';
COMMENT ON COLUMN Day.close_price IS '时段收盘价（示例：12345.678）';
COMMENT ON COLUMN Day.high_price  IS '时段最高成交价';
COMMENT ON COLUMN Day.low_price   IS '时段最低成交价';
COMMENT ON COLUMN Day.open_price  IS '时段开盘价';

CREATE TABLE STOCK_INFO (
    stock_code     VARCHAR2(6),    -- 证券代码（6位字符）
    max_trade_date DATE,           -- 最大的交易日期时间
    stock_name     VARCHAR2(50),   -- 证券名称（最长50字符）
    memo           VARCHAR2(50),   -- 备考
    -- 主键定义（索引自动创建到STOCK_IDX表空间）
    CONSTRAINT pk_STOCK_INFO PRIMARY KEY (stock_code)
    USING INDEX TABLESPACE STOCK_IDX
)
TABLESPACE STOCK_DATA;  -- 替换为实际数据表空间名

-- 添加表注释
COMMENT ON TABLE STOCK_INFO IS '证券一览表';

-- 添加字段注释
COMMENT ON COLUMN STOCK_INFO.stock_code      IS '证券唯一代码（如：600000）';
COMMENT ON COLUMN STOCK_INFO.max_trade_date  IS '最大的交易日期时间';
COMMENT ON COLUMN STOCK_INFO.stock_name      IS '证券完整名称（如：浦发银行）';
COMMENT ON COLUMN STOCK_INFO.memo            IS '备考';


CREATE TABLE LOG (
    log_date       DATE,           
    log_info       VARCHAR2(1000)
)
TABLESPACE STOCK_DATA;

CREATE INDEX idx_log ON LOG(log_date)
TABLESPACE STOCK_IDX;

-- 添加表注释
COMMENT ON TABLE LOG IS 'Log表';

-- 添加字段注释
COMMENT ON COLUMN LOG.log_date  IS 'Log时间';
COMMENT ON COLUMN LOG.log_info  IS 'Log内容';


--==============================================================================
TRUNCATE TABLE log ;
select * from log t order by t.log_date desc

--查询未成功的数据
select t1.code from (select substr(a.log_info, 14, 6) as code from log a  where a.log_info like 'Start%') t1
where t1.code not in (select substr(a.log_info, 12, 6) from log a where a.log_info like 'End%')

--删除M5未成功的数据
delete from m5 t where t.stock_code IN (
select t1.code from (select substr(a.log_info, 14, 6) as code from log a  where a.log_info like 'Start%') t1
where t1.code not in (select substr(a.log_info, 12, 6) from log a where a.log_info like 'End%') )

--==========================
SELECT 
    df.tablespace_name "表空间名称",
    df.bytes/1024/1024 "总大小(MB)",
    (df.bytes - fs.bytes)/1024/1024 "已使用(MB)",
    fs.bytes/1024/1024 "剩余空间(MB)",
    ROUND(100 * (df.bytes - fs.bytes) / df.bytes) "使用率(%)"
FROM 
    (SELECT tablespace_name, SUM(bytes) bytes 
     FROM dba_data_files 
     GROUP BY tablespace_name) df,
    (SELECT tablespace_name, SUM(bytes) bytes 
     FROM dba_free_space 
     GROUP BY tablespace_name) fs
WHERE 
    df.tablespace_name = fs.tablespace_name
ORDER BY 
    "使用率(%)" DESC;