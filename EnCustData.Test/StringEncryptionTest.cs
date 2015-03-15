using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EnCustData.Test
{[TestFixture]
    public class StringEncryptionTest
    {
    [Test]
    public void EncryptAndDecryptAString( )
    {
        //Arrange
        const string passPhrase = "Ni_talar_bra_latin";
        // http://www.hollyfame.com/top-10-greatest-kept-hollywood-secrets-of-all-time/10/
        const string secretString = "Most Pop Stars Lip-sync Onstage";
        string encryptedStr;
        string decryptedStr;
        //Act
       encryptedStr= AESGCM.SimpleEncryptWithPassword(password: passPhrase, secretMessage: secretString);
        decryptedStr = AESGCM.SimpleDecryptWithPassword(encryptedMessage: encryptedStr, password: passPhrase);
        //Assert
        Assert.AreEqual(secretString, decryptedStr, "String should be same after decryption.");
    }
     [Test]
    public void EncryptAndDecryptAStringUTF()
    {
        //Arrange
        const string passPhrase = "كنت أتكلم جيد اللاتينية";
        // http://www.hollyfame.com/top-10-greatest-kept-hollywood-secrets-of-all-time/10/
        const string secretString = "معظم البوب نجوم الشفاه مزامنة على خشبة المسرح";
        string encryptedStr;
        string decryptedStr;
        //Act
        encryptedStr = AESGCM.SimpleEncryptWithPassword(password: passPhrase, secretMessage: secretString);
        decryptedStr = AESGCM.SimpleDecryptWithPassword(encryptedMessage: encryptedStr, password: passPhrase);
        //Assert
        Assert.AreEqual(secretString, decryptedStr, "String should be same after decryption.");
    }
    }
}
