using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;


namespace FirmarXades
{
    public static class Firmar
    {
        public static X509Certificate2Collection EscogerCertificados()
        {
            // Abro el contenedor de certificados X.509 del usuario actual
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            // Pongo todos los certificados en un contenedor 
            X509Certificate2Collection certificates = store.Certificates;
            X509Certificate2Collection foundCertificates = certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
            X509Certificate2Collection selectedCertificates = X509Certificate2UI.SelectFromCollection(foundCertificates,
                "Selecciona un certificado.",
                "Selecciona un certificado de la siguiente lista:",
                X509SelectionFlag.SingleSelection);

            return selectedCertificates;
        }

        public static void FirmarXml(XmlDocument xmlDoc, X509Certificate2 cert)
        {
            // Precondiciones.
            if (xmlDoc == null)
                throw new ArgumentException("xmlDoc");
            if (cert == null)
                throw new ArgumentException("Cert");

            // Creo el objeto de la firma.
            SignedXml signedXml = new SignedXml(xmlDoc);

            // Añado la clave privada.
            signedXml.SigningKey = cert.PrivateKey;

            // Creo una referencia al documento, se pasa "" para decir que es todo el documento
            Reference reference = new Reference();
            reference.Uri = ""; 

            // Añado transformacion a enveloped a la referencia.
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Añado la referencia al objeto de la firma.
            signedXml.AddReference(reference);


            // Añado la informacion del certificado
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));

            signedXml.KeyInfo = keyInfo;

            // Proceso la firma.
            signedXml.ComputeSignature();

            // Obtengo la representación de la firma en Xml.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Añado el xml de la firma al documento original.
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        }


        public static void FirmarXadesEPES(XmlDocument xmlDoc, X509Certificate2 cert)
        {
            // Precondiciones.
            if (xmlDoc == null)
                throw new ArgumentException("xmlDoc");
            if (cert == null)
                throw new ArgumentException("Cert");

            //
            String keyInfoID = "keyinfoID";
            String signedPropertiestypeID = "SignedPropertiestypeID";
            String signatureID = "FacturaeSignatureID";

            // Creo el objeto de la firma.
            XadesSignedXml signedXml = new XadesSignedXml(xmlDoc);

            // Añado la clave privada.
            signedXml.SigningKey = cert.PrivateKey;

            // Creo una referencia al documento, se pasa "" para decir que es todo el documento
            Reference reference = new Reference();
            reference.Uri = "";
            // Añado transformacion a enveloped a la referencia.
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            // Añado la referencia al objeto de la firma.
            signedXml.AddReference(reference);

            // Creo una referencia al keyInfo
            Reference keyInfoReference = new Reference();
            keyInfoReference.Uri = "#" + keyInfoID;
            signedXml.AddReference(keyInfoReference);

            //referencia al SignedProperiestype
            Reference signedProperiestypeReference = new Reference();
            signedProperiestypeReference.Uri = "#" + signedPropertiestypeID;
            signedProperiestypeReference.Type = "http://uri.etsi.org/01903#SignedProperties";
            signedProperiestypeReference.AddTransform(new XmlDsigExcC14NTransform());
            signedXml.AddReference(signedProperiestypeReference);


 
            // Añado la informacion del certificado
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.Id = keyInfoID;
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;


            //info extra para xades-epes
            QualifyingPropertiesType qualifyingProperties = new QualifyingPropertiesType();

            qualifyingProperties.Target = "#" + signatureID;
            qualifyingProperties.SignedProperties = new SignedPropertiesType();
            qualifyingProperties.SignedProperties.Id = signedPropertiestypeID;
            qualifyingProperties.SignedProperties.SignedSignatureProperties = new SignedSignaturePropertiesType();
            qualifyingProperties.SignedProperties.SignedSignatureProperties.SigningTime = DateTime.Today;
            qualifyingProperties.SignedProperties.SignedSignatureProperties.SignaturePolicyIdentifier = new SignaturePolicyIdentifierType();

            SignaturePolicyIdType signaturePolicyIdType = new SignaturePolicyIdType();
            signaturePolicyIdType.SigPolicyId =new ObjectIdentifierType();
            signaturePolicyIdType.SigPolicyId.Identifier=new IdentifierType();
            signaturePolicyIdType.SigPolicyId.Identifier.Value = "http://www.facturae.es/politica_de_firma_formato_facturae/politica_de_firma_formato_facturae_v3_1.pdf";
            signaturePolicyIdType.SigPolicyId.Description = "facturae31";

            signaturePolicyIdType.SigPolicyHash = new DigestAlgAndValueType();
            signaturePolicyIdType.SigPolicyHash.DigestMethod = new DigestMethodType();
            signaturePolicyIdType.SigPolicyHash.DigestMethod.Algorithm = "http://www.w3.org/2000/09/xmldsig#sha1";
            signaturePolicyIdType.SigPolicyHash.DigestValue = Convert.FromBase64String("Ohixl6upD6av8N7pEvDABhEL6hM=");

            qualifyingProperties.SignedProperties.SignedSignatureProperties.SignaturePolicyIdentifier.Item = signaturePolicyIdType;


            signedXml.AddQualifyingPropertiesObject(qualifyingProperties);

                        

            // Proceso la firma.
            signedXml.ComputeSignature();

            // Obtengo la representación de la firma en Xml.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Añado el xml de la firma al documento original.
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        }

        
    }

}

