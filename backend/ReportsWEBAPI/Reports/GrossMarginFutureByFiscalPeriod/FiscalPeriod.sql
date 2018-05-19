--DECLARE @startdate AS DATETIME = getdate()

SELECT TOP (3) PeriodEndDate, FiscalPeriod
FROM _NoLock_FS_FiscalPeriod
--WHERE @startdate  <= PeriodEndDate
ORDER BY PeriodEndDate