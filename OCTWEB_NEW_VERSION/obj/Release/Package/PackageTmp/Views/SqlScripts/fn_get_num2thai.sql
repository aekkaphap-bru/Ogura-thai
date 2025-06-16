USE [OCTWEBTEST]
GO

/****** Object:  UserDefinedFunction [dbo].[get_Num2Thai]    Script Date: 10/28/2022 1:07:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[get_Num2Thai] (@Number1 Money)
RETURNS VARCHAR(MAX)
AS BEGIN
DECLARE @number Numeric(38 , 0)
DECLARE @decimal INT
DECLARE @loops INT
DECLARE @bigLoops INT
DECLARE @counter INT
DECLARE @bigCount INT
DECLARE @mod INT
DECLARE @numbersTable TABLE (number CHAR(1), word VARCHAR(10))
DECLARE @numbersDigit TABLE (number CHAR(1), word VARCHAR(10))
DECLARE @inputNumber VARCHAR(38)
DECLARE @inputNumber1 VARCHAR(38)
DECLARE @inputDecimal VARCHAR(2)
DECLARE @charNumber CHAR(1)
DECLARE @outputString VARCHAR(8000)
DECLARE @outputString1 VARCHAR(8000)
DECLARE @outputChar VARCHAR(10)
DECLARE @outputChar1 VARCHAR(10)
DECLARE @nextNumber CHAR(1)

IF @number1 = 0 RETURN 'ศูนย์บาท'

-- insert data for the numbers and words
INSERT INTO @NumbersTable
SELECT ' ', '' UNION ALL SELECT '0', ''
UNION ALL SELECT '1', 'หนึ่ง' UNION ALL SELECT '2', 'สอง'
UNION ALL SELECT '3', 'สาม' UNION ALL SELECT '4', 'สี่'
UNION ALL SELECT '5', 'ห้า' UNION ALL SELECT '6', 'หก'
UNION ALL SELECT '7', 'เจ็ด' UNION ALL SELECT '8', 'แปด'
UNION ALL SELECT '9', 'เก้า'

INSERT INTO @NumbersDigit
SELECT '1', '' UNION ALL SELECT '2', 'สิบ'
UNION ALL SELECT '3', 'ร้อย' UNION ALL SELECT '4', 'พัน'
UNION ALL SELECT '5', 'หมื่น' UNION ALL SELECT '6', 'แสน'

SET @number = FLOOR(@number1)
SET @decimal = FLOOR((@number1 - FLOOR(@number1))* 100)
SET @inputNumber1 = CONVERT(VARCHAR(38), @number)
SET @inputDecimal = CONVERT(VARCHAR(2), @decimal)
SET @bigLoops = FLOOR(LEN(@inputNumber1) / 6) + 1
SET @mod = LEN(@inputNumber1) % 6
SET @bigCount = 1
SET @outputString = ''

WHILE @bigCount <= @bigLoops BEGIN
IF @bigCount = 1 BEGIN
SET @inputNumber = LEFT(@inputNumber1,@mod)
SET @inputNumber1 = RIGHT(@inputNumber1,LEN(@inputNumber1)-@mod)
END
ELSE BEGIN
SET @inputNumber = LEFT(@inputNumber1,6)
IF @bigCount < @bigLoops
SET @inputNumber1 = RIGHT(@inputNumber1,LEN(@inputNumber1)-6)
END

SET @outputString1 = ''
SET @counter = 1
SET @loops = LEN(@inputNumber)
SET @nextNumber = ''
WHILE 1 <= @loops BEGIN
SET @charNumber = SUBSTRING(@inputNumber,@loops,1)
SET @nextNumber = SUBSTRING(@inputNumber,@loops-1,1)
SELECT @outputChar = word FROM @NumbersTable
WHERE @charNumber = number
SELECT @outputChar1 = word FROM @NumbersDigit
WHERE CONVERT(CHAR(1),@counter) = number
IF @charNumber = '1' AND LEN(@inputNumber) > 1 AND @counter = 1 AND @nextNumber > '0'
SET @outputChar = 'เอ็ด'
IF @charNumber = '1' AND LEN(@inputNumber) >= 2 AND @counter = 2 SET @outputChar = ''
IF @charNumber = '2' AND LEN(@inputNumber) >= 2 AND @counter = 2 SET @outputChar = 'ยี่'
IF @charNumber = '0' SET @outputChar1 = ''
SELECT @outputString1 = @outputChar + @outputChar1 + @outputString1,
@counter = @counter + 1,
@loops = @loops - 1
END

IF @bigCount < @bigLoops
IF @outputString1 <> '' SET @outputString = @outputString + @outputString1 + 'ล้าน'
IF @bigCount >= @bigLoops SET @outputString = @outputString + @outputString1 + 'บาท'
SET @bigCount = @bigCount + 1
END
-- Decimal
IF LEN(@inputDecimal)= 1 SET @inputDecimal = '0' + @inputDecimal
SET @inputNumber = @inputDecimal
SET @counter = 1
SET @loops = LEN(@inputNumber)
SET @outputString1 = ''
SET @nextNumber = SUBSTRING(@inputNumber,@loops-1,1)
WHILE 1 <= @loops BEGIN
SET @charNumber = SUBSTRING(@inputNumber,@loops,1)
SELECT @outputChar = word FROM @NumbersTable
WHERE @charNumber = number
SELECT @outputChar1 = word FROM @NumbersDigit
WHERE CONVERT(CHAR(1),@counter) = number
IF @charNumber = '1' AND LEN(@inputNumber) > 1 AND @counter = 1 AND @nextNumber > '0'
SET @outputChar = 'เอ็ด'
IF @charNumber = '1' AND LEN(@inputNumber) >= 2 AND @counter = 2 SET @outputChar = ''
IF @charNumber = '2' AND LEN(@inputNumber) >= 2 AND @counter = 2 SET @outputChar = 'ยี่'
IF @charNumber = '0' SET @outputChar1 = ''
SELECT @outputString1 = @outputChar + @outputChar1 + @outputString1,
@counter = @counter + 1,
@loops = @loops - 1
END
IF @inputDecimal = '00'
SET @outputString = @outPutString + 'ถ้วน'
ELSE SET @outputString = @outputString + @outputString1 + 'สตางค์'

RETURN @outputString -- return the result
END 
GO


