using System;
using System.Configuration;

namespace Common
{
	public static class SAPCompany
	{
		private static SAPbobsCOM.Company _company;
		public static SAPbobsCOM.Company DICompany { get { return _company; } }

		static SAPCompany()
		{
			ConnectToCompany();
		}

		private static void ConnectToCompany()
		{
			string licenseServer = ConfigurationManager.AppSettings["sapLicenseServer"];
			string dbType = ConfigurationManager.AppSettings["sapDBType"];
			string server = ConfigurationManager.AppSettings["sapServer"];
			string userName = ConfigurationManager.AppSettings["sapUserName"];
			string password = ConfigurationManager.AppSettings["sapPassword"];
			string dbUserName = ConfigurationManager.AppSettings["sapDBUserName"];
			string dbPassword = ConfigurationManager.AppSettings["sapDBPassword"];
			string companyDb = ConfigurationManager.AppSettings["sapCompanyDBName"];

			int result;

			if (DICompany != null)
			{
				if (DICompany.Connected)
				{
					// disconnect
					DICompany.Disconnect();
				}
			}

			_company = new SAPbobsCOM.Company();

			_company.UseTrusted = false;
			_company.language = SAPbobsCOM.BoSuppLangs.ln_Korean_Kr;
			_company.DbServerType = (SAPbobsCOM.BoDataServerTypes)Enum.Parse(typeof(SAPbobsCOM.BoDataServerTypes), dbType);
			_company.LicenseServer = licenseServer;
			_company.Server = server;
			_company.DbUserName = dbUserName;
			_company.DbPassword = dbPassword;
			_company.CompanyDB = companyDb;
			_company.UserName = userName;
			_company.Password = password;

			result = _company.Connect();

			if (result != 0)
			{
				int errorCode;
				string errorString;
				_company.GetLastError(out errorCode, out errorString);

				throw new ApplicationException(string.Format("Error Code: {0}\nError Message: {1}", errorCode, errorString));
			}
		}
	}
}
