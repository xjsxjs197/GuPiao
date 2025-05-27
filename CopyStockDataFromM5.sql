CREATE OR REPLACE PROCEDURE CopyStockDataFromM5 (
) AS
    CURSOR c_stocks IS SELECT stock_code FROM STOCK_INFO ORDER BY stock_code ;
    
    v_max_m30_date  DATE;
    v_max_day_date  DATE;
    v_error_msg     VARCHAR2(4000);
    
BEGIN

    FOR stock_rec IN c_stocks LOOP
        BEGIN
            -- 获取两个表的最大日期（处理NULL情况）
            SELECT NVL(MAX(trade_date), TO_DATE('19700101000000','YYYYMMDDHH24MISS'))
            INTO v_max_m30_date
            FROM M30 
            WHERE stock_code = stock_rec.stock_code;

            SELECT NVL(MAX(trade_date), TO_DATE('19700101','YYYYMMDD'))
            INTO v_max_day_date
            FROM Day 
            WHERE stock_code = stock_rec.stock_code;

            -- 插入M30表（批量操作）
            INSERT INTO M30
            SELECT * FROM M5
            WHERE stock_code = stock_rec.stock_code
            AND TO_CHAR(trade_date , 'MI') IN ('00', '30')
            AND trade_date > v_max_m30_date;

            -- 插入Day表（批量操作）
            INSERT INTO Day
            SELECT * FROM M5
            WHERE stock_code = stock_rec.stock_code
            AND trade_date > v_max_day_date;

            COMMIT;

        EXCEPTION
            WHEN OTHERS THEN
                ROLLBACK;
                log_error(stock_rec.stock_code, SQLERRM);
                v_error_count := v_error_count + 1;
        END;
    END LOOP;

    -- 如果全部成功返回null
    IF v_error_count = 0 THEN
        p_error_codes := NULL;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        log_error('GLOBAL', SQLERRM);
END ProcessStockData;
/
