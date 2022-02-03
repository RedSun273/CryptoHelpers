using SlowCryptoLib;
using System;
using System.Collections.Generic;

namespace CryptoHelpers
{
    public class CryptoHelper : ICryptoHelper
    {
        private readonly IStore store;
        private readonly Dictionary<ICertificate, Tuple<IEnumerator<ICertificateParam>, List<ICertificateParam>>> receivedCertificateParams;
        private readonly List<ICertificate> receivedCertificatesList;
        private IEnumerator<ICertificate> certificateEnumerator;

        public CryptoHelper(IStore store)
        {
            this.store = store;
            receivedCertificateParams = new Dictionary<ICertificate, Tuple<IEnumerator<ICertificateParam>, List<ICertificateParam>>>();
            receivedCertificatesList = new List<ICertificate>();
            certificateEnumerator = store.Certificates.GetEnumerator();
        }

        public byte[] Sign(byte[] data, string certParamValue)
        {
            return FindCertificate(certParamValue).Sign(data);
        }

        public bool Verify(byte[] signature, string certParamValue)
        {
            return FindCertificate(certParamValue).Verify(signature);
        }

        private ICertificate FindCertificate(string certParamValue)
        {
            foreach (var certificate in receivedCertificateParams.Keys)
            {
                var found = FindParam(certificate, certParamValue, true);
                if (found)
                {
                    return certificate;
                }
            }
            var canMoveNext = true;
            while (canMoveNext)
            {
                try
                {
                    canMoveNext = certificateEnumerator.MoveNext();
                    if (!receivedCertificatesList.Contains(certificateEnumerator.Current))
                    {
                        receivedCertificatesList.Add(certificateEnumerator.Current);
                    }

                    var found = FindParam(certificateEnumerator.Current, certParamValue, false);
                    if (found)
                        return certificateEnumerator.Current;
                }
                catch
                {
                    certificateEnumerator = store.Certificates.GetEnumerator();
                    throw;
                }
            }
            throw new Exception("Param not found");
        }

        private bool FindParam(ICertificate certificate, string certParamValue, bool certificateReceived)
        {
            IEnumerator<ICertificateParam> paramEnumerator;
            Tuple<IEnumerator<ICertificateParam>, List<ICertificateParam>> recivedCertificateParams;
            if (certificateReceived)
            {
                recivedCertificateParams = receivedCertificateParams[certificate];
                paramEnumerator = recivedCertificateParams.Item1;

                foreach (var param in recivedCertificateParams.Item2)
                {
                    if (param.Is(certParamValue))
                    {
                        return true;
                    }
                }
            }
            else
            {
                paramEnumerator = certificate.CertificateParams.GetEnumerator();
                recivedCertificateParams = Tuple.Create(paramEnumerator, new List<ICertificateParam>());
                receivedCertificateParams[certificate] = recivedCertificateParams;
            }

            var canMoveNext = true;
            while (canMoveNext)
            {
                try
                {
                    canMoveNext = paramEnumerator.MoveNext();
                    if (!canMoveNext)
                    {
                        break;
                    }
                    recivedCertificateParams.Item2.Add(paramEnumerator.Current);

                    if (paramEnumerator.Current.Is(certParamValue))
                    {
                        return true;
                    }
                }
                catch
                {
                    paramEnumerator = certificate.CertificateParams.GetEnumerator();
                    throw;
                }
            }
            return false;
        }

        public void Dispose()
        {
            foreach (var certificate in receivedCertificatesList)
            {
                certificate.Dispose();
            }
            store.Dispose();
        }
    }
}