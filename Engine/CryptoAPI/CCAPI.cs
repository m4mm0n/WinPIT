using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine.CryptoAPI
{
    public class CCAPI
    {
        #region CONSTS


        // #define PRIVATEKEYBLOB		0x7

        public const Int32 PRIVATEKEYBLOB = 0x7;


        // #define AT_KEYEXCHANGE		1

        public const Int32 AT_KEYEXCHANGE = 1;


        // #define AT_SIGNATURE		2

        public const Int32 AT_SIGNATURE = 2;


        //#define CRYPT_E_NOT_FOUND		_HRESULT_TYPEDEF_(0x80092004L)

        public const Int32 CRYPT_E_NOT_FOUND = -2146885628;


        // #define CERT_PERSONAL_STORE_NAME      L"My"

        public const string CERT_PERSONAL_STORE_NAME = "My";


        // #define CERT_COMPARE_ANY		0

        public const Int32 CERT_COMPARE_ANY = 0;


        // #define CERT_COMPARE_NAME   2

        public const Int32 CERT_COMPARE_NAME = 2;


        // #define CERT_INFO_SUBJECT_FLAG  7

        public const Int32 CERT_INFO_SUBJECT_FLAG = 7;


        // #define CERT_COMPARE_SHIFT        16

        public const Int32 CERT_COMPARE_SHIFT = 16;


        // #define CERT_FIND_SUBJECT_NAME    (CERT_COMPARE_NAME << CERT_COMPARE_SHIFT | CERT_INFO_SUBJECT_FLAG)

        public const Int32 CERT_FIND_SUBJECT_NAME =

            (CERT_COMPARE_NAME << CERT_COMPARE_SHIFT) | CERT_INFO_SUBJECT_FLAG;


        // #define CERT_FIND_ANY	(CERT_COMPARE_ANY << CERT_COMPARE_SHIFT)

        public const Int32 CERT_FIND_ANY = CERT_COMPARE_ANY << CERT_COMPARE_SHIFT;


        // #define CERT_COMPARE_NAME_STR_W     8

        public const Int32 CERT_COMPARE_NAME_STR_W = 8;


        // #define CERT_FIND_SUBJECT_STR_W   \

        //   (CERT_COMPARE_NAME_STR_W << CERT_COMPARE_SHIFT | CERT_INFO_SUBJECT_FLAG)

        public const Int32 CERT_FIND_SUBJECT_STR_W =

            (CERT_COMPARE_NAME_STR_W << CERT_COMPARE_SHIFT) | CERT_INFO_SUBJECT_FLAG;


        // #define CERT_FIND_SUBJECT_STR CERT_FIND_SUBJECT_STR_W

        public const Int32 CERT_FIND_SUBJECT_STR = CERT_FIND_SUBJECT_STR_W;


        // #define CERT_STORE_PROV_SYSTEM_W      ((LPCSTR) 10)

        public const Int32 CERT_STORE_PROV_SYSTEM_W = 10;


        // #define CERT_STORE_PROV_SYSTEM        CERT_STORE_PROV_SYSTEM_W

        public const Int32 CERT_STORE_PROV_SYSTEM = CERT_STORE_PROV_SYSTEM_W;


        // #define CERT_SYSTEM_STORE_CURRENT_USER_ID     1

        public const Int32 CERT_SYSTEM_STORE_CURRENT_USER_ID = 1;


        // #define CERT_SYSTEM_STORE_LOCATION_SHIFT      16

        public const Int32 CERT_SYSTEM_STORE_LOCATION_SHIFT = 16;


        // #define CERT_SYSTEM_STORE_CURRENT_USER        \

        //   (CERT_SYSTEM_STORE_CURRENT_USER_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT)

        public const Int32 CERT_SYSTEM_STORE_CURRENT_USER =

            CERT_SYSTEM_STORE_CURRENT_USER_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT;


        // #define CERT_CLOSE_STORE_CHECK_FLAG       0x00000002

        public const Int32 CERT_CLOSE_STORE_CHECK_FLAG = 0x00000002;


        // #define ALG_CLASS_HASH                  (4 << 13)

        // #define ALG_TYPE_ANY                    (0)

        // #define ALG_SID_SHA1                    4

        // #define CALG_SHA1               (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_SHA1)

        public const Int32 CALG_SHA1 = (4 << 13) | 4;


        // #define ALG_CLASS_SIGNATURE             (1 << 13)

        // #define ALG_TYPE_RSA                    (2 << 9)

        // #define ALG_SID_RSA_ANY                 0

        // #define CALG_RSA_SIGN           (ALG_CLASS_SIGNATURE | ALG_TYPE_RSA | ALG_SID_RSA_ANY)

        public const Int32 CALG_RSA_SIGN = (1 << 13) | (2 << 9);


        // #define PROV_RSA_FULL           1

        public const Int32 PROV_RSA_FULL = 0x00000001;


        // #define CRYPT_VERIFYCONTEXT     0xF0000000

        public const Int32 CRYPT_VERIFYCONTEXT = -268435456; //No private key access required


        // #define X509_ASN_ENCODING           0x00000001

        public const Int32 X509_ASN_ENCODING = 0x00000001;


        // #define PKCS_7_ASN_ENCODING         0x00010000

        public const Int32 PKCS_7_ASN_ENCODING = 0x00010000;


        // #define MY_TYPE       (PKCS_7_ASN_ENCODING | X509_ASN_ENCODING)

        public const Int32 MY_TYPE = PKCS_7_ASN_ENCODING | X509_ASN_ENCODING;


        // #define HP_HASHVAL              0x0002

        public const Int32 HP_HASHVAL = 0x00000002;


        // #define HP_HASHSIZE             0x0004

        public const Int32 HP_HASHSIZE = 0x00000004;


        // #define PUBLICKEYBLOBEX 0xA

        public const Int32 PUBLICKEYBLOBEX = 0x0A;


        // #define PUBLICKEYBLOB           0x6

        public const Int32 PUBLICKEYBLOB = 0x06;


        // #define CUR_BLOB_VERSION 0x02

        public const Int32 CUR_BLOB_VERSION = 0x02;


        // #define CRYPT_EXPORTABLE        0x00000001

        public const Int32 CRYPT_EXPORTABLE = 0x00000001;


        // #define szOID_RSA_RSA           "1.2.840.113549.1.1.1"

        public const String szOID_RSA_RSA = "1.2.840.113549.1.1.1";


        // #define szOID_RSA_MD5           "1.2.840.113549.2.5"

        public const String szOID_RSA_MD5 = "1.2.840.113549.2.5";


        // #define szOID_RSA_MD5RSA        "1.2.840.113549.1.1.4"

        public const String szOID_RSA_MD5RSA = "1.2.840.113549.1.1.4";


        // #define szOID_OIWSEC_sha1       "1.3.14.3.2.26"

        public const String szOID_OIWSEC_sha1 = "1.3.14.3.2.26";


        // #define RSA_CSP_PUBLICKEYBLOB               ((LPCSTR) 19)

        public const Int32 RSA_CSP_PUBLICKEYBLOB = 19;


        // #define X509_PUBLIC_KEY_INFO                ((LPCSTR) 8)

        public const Int32 X509_PUBLIC_KEY_INFO = 8;


        #endregion
        #region STRUCTS


        // typedef struct _PUBLICKEYSTRUC

        // {

        //		BYTE bType;

        //		BYTE bVersion;

        //		WORD reserved;

        //		ALG_ID aiKeyAlg;

        // } BLOBHEADER, PUBLICKEYSTRUC;

        [StructLayout(LayoutKind.Sequential)]
        public struct PUBLICKEYSTRUC

        {

            public Byte bType;

            public Byte bVersion;

            public Int16 reserved;

            public Int32 aiKeyAlg;

        }


        // typedef struct _RSAPUBKEY

        // {

        //		DWORD magic;

        //		DWORD bitlen;

        //		DWORD pubexp;

        // } RSAPUBKEY;

        [StructLayout(LayoutKind.Sequential)]
        public struct RSAPUBKEY

        {

            public Int32 magic;

            public Int32 bitlen;

            public Int32 pubexp;

        }


        // typedef struct _CRYPTOAPI_BLOB

        // {

        //		DWORD   cbData;

        //		BYTE    *pbData;

        // } CRYPT_HASH_BLOB, CRYPT_INTEGER_BLOB,

        //   CRYPT_OBJID_BLOB, CERT_NAME_BLOB;

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPTOAPI_BLOB

        {

            public Int32 cbData;

            public Byte[] pbData;

        }


        // typedef struct _CRYPT_ALGORITHM_IDENTIFIER

        // {

        //		LPSTR pszObjId;

        //		CRYPT_OBJID_BLOB Parameters;

        // } CRYPT_ALGORITHM_IDENTIFIER;

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_ALGORITHM_IDENTIFIER

        {

            [MarshalAs(UnmanagedType.LPStr)] public String pszObjId;

            public CRYPTOAPI_BLOB Parameters;

        }


        // typedef struct _CRYPT_SIGN_MESSAGE_PARA

        // {

        //		DWORD cbSize;

        //		DWORD dwMsgEncodingType;

        //		PCCERT_CONTEXT pSigningCert;

        //		CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;

        //		void *pvHashAuxInfo;

        //		DWORD cMsgCert;

        //		PCCERT_CONTEXT *rgpMsgCert;

        //		DWORD cMsgCrl;

        //		PCCRL_CONTEXT *rgpMsgCrl;

        //		DWORD cAuthAttr;

        //		PCRYPT_ATTRIBUTE rgAuthAttr;

        //		DWORD cUnauthAttr;

        //		PCRYPT_ATTRIBUTE rgUnauthAttr;

        //		DWORD dwFlags;

        //		DWORD dwInnerContentType;

        // } CRYPT_SIGN_MESSAGE_PARA;

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_SIGN_MESSAGE_PARA

        {

            public Int32 cbSize;

            public Int32 dwMsgEncodingType;

            public IntPtr pSigningCert;

            public CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;

            public IntPtr pvHashAuxInfo;

            public Int32 cMsgCert;

            public IntPtr rgpMsgCert;

            public Int32 cMsgCrl;

            public IntPtr rgpMsgCrl;

            public Int32 cAuthAttr;

            public IntPtr rgAuthAttr;

            public Int32 cUnauthAttr;

            public IntPtr rgUnauthAttr;

            public Int32 dwFlags;

            public Int32 dwInnerContentType;

        }


        // typedef struct _CRYPT_VERIFY_MESSAGE_PARA

        // {

        //		DWORD cbSize;

        //		DWORD dwMsgAndCertEncodingType;

        //		HCRYPTPROV hCryptProv;

        //		PFN_CRYPT_GET_SIGNER_CERTIFICATE pfnGetSignerCertificate;

        //		void *pvGetArg;

        // } CRYPT_VERIFY_MESSAGE_PARA;

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_VERIFY_MESSAGE_PARA

        {

            public Int32 cbSize;

            public Int32 dwMsgAndCertEncodingType;

            public IntPtr hCryptProv;

            public IntPtr pfnGetSignerCertificate;

            public IntPtr pvGetArg;

        }


        // typedef struct _CRYPT_BIT_BLOB

        // {

        //		DWORD cbData;

        //		BYTE *pbData;

        //		DWORD cUnusedBits;

        // } CRYPT_BIT_BLOB;

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_BIT_BLOB

        {

            public Int32 cbData;

            public IntPtr pbData;

            public Int32 cUnusedBits;

        }


        // typedef struct _CERT_PUBLIC_KEY_INFO

        // {

        //		CRYPT_ALGORITHM_IDENTIFIER Algorithm;

        //		CRYPT_BIT_BLOB PublicKey;

        // } CERT_PUBLIC_KEY_INFO;

        [StructLayout(LayoutKind.Sequential)]
        public struct CERT_PUBLIC_KEY_INFO

        {

            public CRYPT_ALGORITHM_IDENTIFIER Algorithm;

            public CRYPT_BIT_BLOB PublicKey;

        }


        #endregion
        #region FUNCTIONS (IMPORTS)


        // HCERTSTORE WINAPI CertOpenStore(

        //		LPCSTR lpszStoreProvider,

        //		DWORD dwMsgAndCertEncodingType,

        //		HCRYPTPROV hCryptProv,

        //		DWORD dwFlags,

        //		const void* pvPara

        // );

        [DllImport("Crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CertOpenStore(

            Int32 lpszStoreProvider,

            Int32 dwMsgAndCertEncodingType,

            IntPtr hCryptProv,

            Int32 dwFlags,

            String pvPara

        );


        // HCERTSTORE WINAPI CertOpenSystemStore(

        //		HCRYPTPROV hprov,

        //		LPTCSTR szSubsystemProtocol

        // );

        [DllImport("Crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CertOpenSystemStore(

            IntPtr hprov,

            String szSubsystemProtocol

        );


        // BOOL WINAPI CertCloseStore(

        //		HCERTSTORE hCertStore,

        //		DWORD dwFlags

        // );

        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern Boolean CertCloseStore(

            IntPtr hCertStore,

            Int32 dwFlags

        );


        // BOOL WINAPI CryptAcquireContext(

        //		HCRYPTPROV* phProv,

        //		LPCTSTR pszContainer,

        //		LPCTSTR pszProvider,

        //		DWORD dwProvType,

        //		DWORD dwFlags

        // );

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptAcquireContext(

            ref IntPtr hProv,

            String pszContainer,

            String pszProvider,

            Int32 dwProvType,

            Int32 dwFlags

        );


        // BOOL WINAPI CryptCreateHash(

        //		HCRYPTPROV hProv,

        //		ALG_ID Algid,

        //		HCRYPTKEY hKey,

        //		DWORD dwFlags,

        //		HCRYPTHASH* phHash

        // );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptCreateHash(

            IntPtr hProv,

            Int32 Algid,

            IntPtr hKey,

            Int32 dwFlags,

            ref IntPtr phHash

        );


        // BOOL WINAPI CryptGetHashParam(

        //		HCRYPTHASH hHash,

        //		DWORD dwParam,

        //		BYTE* pbData,

        //		DWORD* pdwDataLen,

        //		DWORD dwFlags

        // );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGetHashParam(

            IntPtr hHash,

            Int32 dwParam,

            ref Int32 pbData,

            ref Int32 pdwDataLen,

            Int32 dwFlags

        );


        // BOOL WINAPI CryptSetHashParam(

        //		HCRYPTHASH hHash,

        //		DWORD dwParam,

        //		BYTE* pbData,

        //		DWORD dwFlags

        // );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptSetHashParam(

            IntPtr hHash,

            Int32 dwParam,

            Byte[] pbData,

            Int32 dwFlags

        );


        // BOOL WINAPI CryptImportPublicKeyInfo(

        //		HCRYPTPROV hCryptProv,

        //		DWORD dwCertEncodingType,

        //		PCERT_PUBLIC_KEY_INFO pInfo,

        //		HCRYPTKEY* phKey

        // );

        [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptImportPublicKeyInfo(

            IntPtr hCryptProv,

            Int32 dwCertEncodingType,

            IntPtr pInfo,

            ref IntPtr phKey

        );


        // BOOL WINAPI CryptImportKey(

        //		HCRYPTPROV hProv,

        //		BYTE* pbData,

        //		DWORD dwDataLen,

        //		HCRYPTKEY hPubKey,

        //		DWORD dwFlags,

        //		HCRYPTKEY* phKey

        // );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptImportKey(

            IntPtr hProv,

            Byte[] pbData,

            Int32 dwDataLen,

            IntPtr hPubKey,

            Int32 dwFlags,

            ref IntPtr phKey

        );


        // BOOL WINAPI CryptVerifySignature(

        //		HCRYPTHASH hHash,

        //		BYTE* pbSignature,

        //		DWORD dwSigLen,

        //		HCRYPTKEY hPubKey,

        //		LPCTSTR sDescription,

        //		DWORD dwFlags

        // );

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptVerifySignature(

            IntPtr hHash,

            Byte[] pbSignature,

            Int32 dwSigLen,

            IntPtr hPubKey,

            String sDescription,

            Int32 dwFlags

        );


        // BOOL WINAPI CryptDestroyKey(

        //		HCRYPTKEY hKey

        // );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptDestroyKey(

            IntPtr hKey

        );


        // BOOL WINAPI CryptDestroyHash(

        //		HCRYPTHASH hHash

        // );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptDestroyHash(

            IntPtr hHash

        );


        // BOOL WINAPI CryptReleaseContext(

        //		HCRYPTPROV hProv,

        //		DWORD dwFlags

        // );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptReleaseContext(

            IntPtr hProv,

            Int32 dwFlags

        );


        // BOOL WINAPI CryptGenKey(

        //		HCRYPTPROV hProv,

        //		ALG_ID Algid,

        //		DWORD dwFlags,

        //		HCRYPTKEY* phKey

        // );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGenKey(

            IntPtr hProv,

            Int32 Algid,

            Int32 dwFlags,

            ref IntPtr phKey

        );


        // BOOL WINAPI CryptExportKey(

        //		HCRYPTKEY hKey,

        //		HCRYPTKEY hExpKey,

        //		DWORD dwBlobType,

        //		DWORD dwFlags,

        //		BYTE* pbData,

        //		DWORD* pdwDataLen

        // );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptExportKey(

            IntPtr hKey,

            IntPtr hExpKey,

            Int32 dwBlobType,

            Int32 dwFlags,

            Byte[] pbData,

            ref Int32 pdwDataLen

        );


        // PCCERT_CONTEXT WINAPI CertFindCertificateInStore(

        //		HCERTSTORE hCertStore,

        //		DWORD dwCertEncodingType,

        //		DWORD dwFindFlags,

        //		DWORD dwFindType,

        //		const void* pvFindPara,

        //		PCCERT_CONTEXT pPrevCertContext

        // );

        [DllImport("Crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CertFindCertificateInStore(

            IntPtr hCertStore,

            Int32 dwCertEncodingType,

            Int32 dwFindFlags,

            Int32 dwFindType,

            String pvFindPara,

            IntPtr pPrevCertContext

        );


        // BOOL WINAPI CertFreeCertificateContext(

        //		PCCERT_CONTEXT pCertContext

        // );

        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern Boolean CertFreeCertificateContext(

            IntPtr pCertContext

        );


        // BOOL WINAPI CryptSignMessage(

        //		PCRYPT_SIGN_MESSAGE_PARA pSignPara,

        //		BOOL fDetachedSignature,

        //		DWORD cToBeSigned,

        //		const BYTE* rgpbToBeSigned[],

        //		DWORD rgcbToBeSigned[],

        //		BYTE* pbSignedBlob,

        //		DWORD* pcbSignedBlob

        // );

        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern Boolean CryptSignMessage(

            ref CRYPT_SIGN_MESSAGE_PARA pSignPara,

            Boolean fDetachedSignature,

            Int32 cToBeSigned,

            IntPtr[] rgpbToBeSigned,

            Int32[] rgcbToBeSigned,

            Byte[] pbSignedBlob,

            ref Int32 pcbSignedBlob

        );


        // BOOL WINAPI CryptVerifyMessageSignature(

        //		PCRYPT_VERIFY_MESSAGE_PARA pVerifyPara,

        //		DWORD dwSignerIndex,

        //		const BYTE* pbSignedBlob,

        //		DWORD cbSignedBlob,

        //		BYTE* pbDecoded,

        //		DWORD* pcbDecoded,

        //		PCCERT_CONTEXT* ppSignerCert

        // );

        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern Boolean CryptVerifyMessageSignature(

            ref CRYPT_VERIFY_MESSAGE_PARA pVerifyPara,

            Int32 dwSignerIndex,

            Byte[] pbSignedBlob,

            Int32 cbSignedBlob,

            Byte[] pbDecoded,

            ref Int32 pcbDecoded,

            IntPtr ppSignerCert

        );


        // BOOL WINAPI CryptEncodeObject(

        //		DWORD dwCertEncodingType,

        //		LPCSTR lpszStructType,

        //		const void* pvStructInfo,

        //		BYTE* pbEncoded,

        //		DWORD* pcbEncoded

        // );

        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern Boolean CryptEncodeObject(

            Int32 dwCertEncodingType,

            Int32 lpszStructType,

            ref CERT_PUBLIC_KEY_INFO pvStructInfo,

            Byte[] pbEncoded,

            ref Int32 pcbEncoded

        );


        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern Boolean CryptEncodeObject(

            Int32 dwCertEncodingType,

            Int32 lpszStructType,

            Byte[] pvStructInfo,

            IntPtr pbEncoded,

            ref Int32 pcbEncoded

        );


        // PCCERT_CONTEXT WINAPI CertCreateCertificateContext(

        //		DWORD dwCertEncodingType,

        //		const BYTE* pbCertEncoded,

        //		DWORD cbCertEncoded

        // );

        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern IntPtr CertCreateCertificateContext(

            Int32 dwCertEncodingType,

            Byte[] pbCertEncoded,

            Int32 cbCertEncoded

        );


        // BOOL WINAPI CryptAcquireCertificatePrivateKey(

        //		PCCERT_CONTEXT pCert,

        //		DWORD dwFlags,

        //		void* pvReserved,

        //		HCRYPTPROV* phCryptProv,

        //		DWORD* pdwKeySpec,

        //		BOOL* pfCallerFreeProv

        // );

        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern Boolean CryptAcquireCertificatePrivateKey(

            IntPtr pCert,

            Int32 dwFlags,

            IntPtr pvReserved,

            ref IntPtr phCryptProv,

            ref Int32 pdwKeySpec,

            ref Boolean pfCallerFreeProv

        );


        // BOOL WINAPI CryptHashData(

        //		HCRYPTHASH hHash,

        //		BYTE* pbData,

        //		DWORD dwDataLen,

        //		DWORD dwFlags

        // );

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern Boolean CryptHashData(

            IntPtr hHash,

            Byte[] pbData,

            Int32 dwDataLen,

            Int32 dwFlags

        );


        // BOOL WINAPI CryptSignHash(

        //		HCRYPTHASH hHash,

        //		DWORD dwKeySpec,

        //		LPCTSTR sDescription,

        //		DWORD dwFlags,

        //		BYTE* pbSignature,

        //		DWORD* pdwSigLen

        // );

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern Boolean CryptSignHash(

            IntPtr hHash,

            Int32 dwKeySpec,

            String sDescription,

            Int32 dwFlags,

            Byte[] pbSignature,

            ref Int32 pdwSigLen

        );


        // BOOL WINAPI CryptGetUserKey(

        //		HCRYPTPROV hProv,

        //		DWORD dwKeySpec,

        //		HCRYPTKEY* phUserKey

        // );

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern Boolean CryptGetUserKey(

            IntPtr hProv,

            Int32 dwKeySpec,

            ref IntPtr phUserKey

        );


        #endregion


    }
}
