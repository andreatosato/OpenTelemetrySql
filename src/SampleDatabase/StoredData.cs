namespace SampleDatabase
{
    public class StoredData
    {
        public int Id { get; set; }
    }
}


/*
 *
CREATE PROCEDURE StoredData
AS
BEGIN
	 SELECT 1 AS Id
END
GO
 */