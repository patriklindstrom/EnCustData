using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Crmf;

namespace EnCustData
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string passPhrase = "Ni_talar_bra_latin";
            string sqlconEncrypt = "Data Source=wsp0740c;Initial Catalog=db1;Integrated Security=True";
            string sqlconTargetData = "Data Source=wsp0740c;Initial Catalog=eCM_Local_DB;Integrated Security=True";
            bool makeUpdate = false;
            var encryptedLst = new List<EncryptedField>();
            System.Console.WriteLine("Database to encrypt {0}", sqlconTargetData);
            var stopW = System.Diagnostics.Stopwatch.StartNew();
            System.Console.WriteLine("Start {0}", DateTime.Now);


            encryptedLst.AddRange(RemoveAllFaxNumbersEtcFromOrg(sqlconTargetData, passPhrase, makeUpdate));


            using (var dbconEncrypt = new SqlConnection(sqlconEncrypt))
            {
                dbconEncrypt.Open();
                // encryptedLst save to dbconEncrypt or textfile or whatever.
                dbconEncrypt.Close();
            }
            stopW.Stop();
            System.Console.WriteLine("Stop {0}", DateTime.Now);
            System.Console.WriteLine("Elapsed {0} seconds", stopW.Elapsed.Seconds);
        }

        // TODO: conTarget should be dependency injected list instead. RemoveAllFaxNumbersEtcFromOrg should not know if it is database.
        private static List<EncryptedField> RemoveAllFaxNumbersEtcFromOrg(string sqlconTargetData, string passW,
            Boolean makeUpdate)
        {
            const string GET_ALL_ROWS = "SELECT OrganizationContactId,ContactName FROM clo_OrganizationContact_TB";
            const string SEARCH_ROW =
                @"SELECT OrganizationContactId,ContactName FROM [dbo].[clo_OrganizationContact_TB] 
                                    WHERE [OrganizationContactId] = @OrganizationContactId ;";
            const string UPDATE_ROW = @"  Update clo_OrganizationContact_TB
                                       SET ContactName = @EncryptedContactName
                                        Where OrganizationContactId = @OrganizationContactId;";
            const string ADD_ENCRYPTED_ROW = @"INSERT INTO [dbo].[EncryptVal_TB]
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

            int maxRows = 1000;
            var encryptedReturnList = new List<EncryptedField>();
            int noOfRows = 0;

            using (var dbconTarget = new SqlConnection(sqlconTargetData))
            {
                dbconTarget.Open();
                using (SqlCommand cmd = new SqlCommand(GET_ALL_ROWS, dbconTarget))
                {
                    //cmd.Parameters.AddWithValue("@uniqueKey", theUniqueKey);
                    //We expect col0 to be Id and col1 to be the value
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        encryptedReturnList.Add(new EncryptedField
                        {
                            DbName = "eCM_Local_DB"
                            ,
                            TblName = "clo_OrganizationContact_TB"
                            ,
                            FieldName = "ContactName"
                            ,
                            ValueKey = Guid.NewGuid()
                            ,
                            ValueOrgKey = reader.GetValue(0).ToString()
                            ,
                            InsertDate = DateTime.UtcNow
                            ,
                            EncryptedValue =
                                AESGCM.SimpleEncryptWithPassword(password: passW, secretMessage: reader.GetString(1))
                        });
                        if (noOfRows >= maxRows)
                        {
                            System.Console.WriteLine("Reached maxrows {0}",maxRows);
                            reader.Close();
                            break;
                        }
                        noOfRows += 1;
                    }
                    reader.Close();
                }
                dbconTarget.Close();
            }
            return encryptedReturnList;
        }

        //    [Id] [int] IDENTITY(1,1) NOT NULL,
        //[DbName] [nvarchar](50) NOT NULL,
        //[TblName] [nvarchar](50) NOT NULL,
        //[FieldName] [nvarchar](50) NOT NULL,
        //[ValueKey] [uniqueidentifier] NOT NULL,
        //[InsertDate] [datetime2](7) NOT NULL,
        //[EncryptedValue] [nvarchar](512) NOT NULL,
        internal class EncryptedField
        {
            public int Id { get; set; }
            public string DbName { get; set; }
            public string TblName { get; set; }
            public string FieldName { get; set; }
            public Guid ValueKey { get; set; }
            public string ValueOrgKey { get; set; }
            public DateTime InsertDate { get; set; }
            public string EncryptedValue { get; set; }
        }
    }
}
