﻿using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Serialization;

namespace fiskaltrust.Middleware.SCU.ES.TicketBAI
{
    public class TicketBaiRequestFactory
    {
        private readonly TicketBaiSCUConfiguration _configuration;

        public TicketBaiRequestFactory(TicketBaiSCUConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateSignedXmlContent<T>(T request)
        {
            var doc = new XmlDocument();
            var nav = doc.CreateNavigator();
            using (var w = nav!.AppendChild())
            {
                var ser = new XmlSerializer(typeof(T));
                ser.Serialize(w, request);
            }

            var privateKey = _configuration.Certificate.GetRSAPrivateKey();

            var keyInfo = new KeyInfo();
            var keyInfoData = new KeyInfoX509Data();
            keyInfoData.AddCertificate(_configuration.Certificate);
            keyInfo.AddClause(keyInfoData);

            var reference = new Reference("");
            var signedXml = new SignedXml(doc)
            {
                SigningKey = privateKey
            };
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            signedXml.KeyInfo = keyInfo;
            signedXml.AddReference(reference);
            signedXml.ComputeSignature();

            var signature = signedXml.GetXml();
            doc.DocumentElement!.AppendChild(signature);

            return doc.OuterXml;
        }
    }
}