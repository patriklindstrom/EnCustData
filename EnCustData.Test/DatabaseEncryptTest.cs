using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EnCustData.Test
{
    [TestFixture]
    internal class DatabaseEncryptTest
    {
        private const string SQL_CON =
            @"Data Source=(LocalDB)\v11.0;AttachDbFilename=D:\Users\Patrik\Documents\db1.mdf;Integrated Security=True;Connect Timeout=30";

        private const string CREATE_TABLES = @"CREATE TABLE [dbo].[EncryptVal_TB](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DbName] [nvarchar](50) NOT NULL,
	[TblName] [nvarchar](50) NOT NULL,
	[FieldName] [nvarchar](50) NOT NULL,
	[ValueKey] [uniqueidentifier] NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL,
	[EncryptedValue] [nvarchar](512) NOT NULL,
    CONSTRAINT [PK_EncryptVal_TB] PRIMARY KEY CLUSTERED 
    (
	[Id] ASC
        )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
    ) ON [PRIMARY]";

        private const string ADD_KEY =
            @"ALTER TABLE [dbo].[EncryptVal_TB]  WITH CHECK ADD  CONSTRAINT [UniqueValueKey] FOREIGN KEY([Id])
REFERENCES [dbo].[EncryptVal_TB] ([Id])";

        private const string ADD_CHECK_CONSTRAINT =
            @"ALTER TABLE [dbo].[EncryptVal_TB] CHECK CONSTRAINT [UniqueValueKey]";

        private const string ADD_CHECK_DEFAULT =
            @"ALTER TABLE [dbo].[EncryptVal_TB] ADD  CONSTRAINT [DF_EncryptVal_TB_InsertDate]  DEFAULT (getdate()) FOR [InsertDate]";
        private const string DROP_TABLE =
           @"DROP TABLE [dbo].[EncryptVal_TB]";

        private const string ADD_ENCRYPTED_ROW = @"INSERT INTO [dbo].[EncryptVal_TB]
           ([DbName]
           ,[TblName]
           ,[FieldName]
           ,[ValueKey]
           ,[EncryptedValue])
     VALUES
           ('Fantasy_DB'
           ,'NonReal_TB'
           ,'ChaosCol1'
           ,@uniqueKey
           ,@encryptedStr)";
        private const string SEARCH_ROW = @"SELECT EncryptedValue from [dbo].[EncryptVal_TB] 
                                    WHERE [ValueKey] = @uniqueKey ;";
       [TestFixtureSetUp]
        public void Init()
        {
            using (SqlConnection con = new SqlConnection(SQL_CON))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(CREATE_TABLES, con))
                {
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd = new SqlCommand(ADD_KEY, con))
                {
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd = new SqlCommand(ADD_CHECK_CONSTRAINT, con))
                {
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd = new SqlCommand(ADD_CHECK_DEFAULT, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

       [TestFixtureTearDown]
        public void Cleanup()
        {
            using (SqlConnection con = new SqlConnection(SQL_CON))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(DROP_TABLE, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [Test]
        public void InsertEncryptedRow()
        {
            //Arrange
            using (SqlConnection con = new SqlConnection(SQL_CON))
            {
                // Specify the parameter value. 
                var theUniqueKey =  Guid.NewGuid().ToString();
                const string passPhrase = "Ni_talar_bra_latin";
                // http://www.hollyfame.com/top-10-greatest-kept-hollywood-secrets-of-all-time/10/
                const string secretString = "Most Pop Stars Lip-sync Onstage";
               
                        var encryptedStr = AESGCM.SimpleEncryptWithPassword(password: passPhrase, secretMessage: secretString);
     

                con.Open();
                using (SqlCommand cmd = new SqlCommand(ADD_ENCRYPTED_ROW, con))
                {
                    cmd.Parameters.AddWithValue("@uniqueKey", theUniqueKey);
                    cmd.Parameters.AddWithValue("@encryptedStr", encryptedStr);
                   cmd.ExecuteNonQuery();
                }

                //Act
                string encryptedReturnVal=String.Empty;
                int noOfRows = 0;
                using (SqlCommand cmd = new SqlCommand(SEARCH_ROW, con))
                {
                   
                    cmd.Parameters.AddWithValue("@uniqueKey", theUniqueKey);
                    var reader = cmd.ExecuteReader();
                     int encCryptCol = reader.GetOrdinal("EncryptedValue");
                    
                    while (reader.Read())
                    {
                        encryptedReturnVal = reader.GetString(encCryptCol);
                        noOfRows += 1;
                    }
                    reader.Close();
                }
                var decryptedStr = AESGCM.SimpleDecryptWithPassword(encryptedMessage: encryptedReturnVal, password: passPhrase);
                //Assert
                Assert.AreEqual(secretString, decryptedStr, "String should be same after decryption from database.");
                Assert.AreEqual(noOfRows, 1,"Should only be one row from uniqe key search");
            }
        }
    }
}
