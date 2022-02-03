using System;
using System.Linq;
using System.Text;
using SlowCryptoLib;

namespace CryptoHelpers
{
    /* В хранилище есть 33 сертификата с такими параметрами

        0. cert_param_0
        1. cert_param_1_0
           cert_param_1_1
           ...
           cert_param_1_28
        2. cert_param_2
        3. cert_param_3
        4. cert_param_4
        5. cert_param_5
        6. cert_param_6
        7. cert_param_7
        8. cert_param_8_0
           cert_param_8_1
           ...
           cert_param_8_11
        9. cert_param_9
       10. cert_param_10
       11. cert_param_11
       12. cert_param_12
       13. cert_param_13
       14. cert_param_14
       15. cert_param_15
       16. cert_param_16
       17. cert_param_17
       18. cert_param_18
       19. cert_param_19
       20. cert_param_20
       21. cert_param_21
       22. cert_param_22_0
           cert_param_22_1
           ...
           cert_param_22_8
       23. cert_param_23
       24. cert_param_24
       25. cert_param_25
       26. cert_param_26
       27. cert_param_27
       28. cert_param_28
       29. cert_param_29
       30. cert_param_30
       31. cert_param_31
       32. cert_param_32
    */
    public static class Program
    {
        private const string CertParam = "cert_param_0";
        private static readonly byte[] DataToSign = Encoding.Unicode.GetBytes("Строка для подписания");

        public static void Main()
        {
            var store = new Store();

            var certificate = store.Certificates.First();
            var firstParam = certificate.CertificateParams.First();
            Console.WriteLine($"Первый сертификат содержит параметр '{CertParam}' ({firstParam.Is(CertParam)}).");

            var signature = certificate.Sign(DataToSign);
            Console.Write($"Подпись: '{Convert.ToBase64String(signature)[..10]}...' ");
            Console.WriteLine($"проходит верификацию ({certificate.Verify(signature)}).");

            Console.WriteLine("Все извлеченные сертификаты должны быть закрыты перед закрытием хранилища.");
            certificate.Dispose();
            store.Dispose();
        }
    }
}