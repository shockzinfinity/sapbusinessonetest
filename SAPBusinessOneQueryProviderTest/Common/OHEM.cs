using System;

namespace Common
{
	[CustomB1Object(B1ObjectType.Table, "OHEM")]
	public class OHEM
	{
		[CustomField("empID")]
		public int empID;
		[CustomField("lastName")]
		public string lastName;
		// TEST
		//[CustomField("lastName")]
		//public string sung;
		[CustomField("firstName")]
		public string firstName;
		[CustomField("middleName")]
		public string middleName;
		[CustomField("sex")]
		public string sex;
		[CustomField("jobTitle")]
		public string jobTitle;
		[CustomField("type")]
		public int? type;
		[CustomField("dept")]
		public int? dept;
		[CustomField("branch")]
		public int? branch;
		[CustomField("workStreet")]
		public string workStreet;
		[CustomField("workBlock")]
		public string workBlock;
		[CustomField("workZip")]
		public string workZip;
		[CustomField("workCity")]
		public string workCity;
		[CustomField("workCounty")]
		public string workCounty;
		[CustomField("workCountr")]
		public string workCountr;
		[CustomField("workState")]
		public string workState;
		[CustomField("manager")]
		public int? manager;
		[CustomField("userId")]
		public int? userId;
		[CustomField("salesPrson")]
		public int? salesPrson;
		[CustomField("officeTel")]
		public string officeTel;
		[CustomField("officeExt")]
		public string officeExt;
		[CustomField("mobile")]
		public string mobile;
		[CustomField("pager")]
		public string pager;
		[CustomField("homeTel")]
		public string homeTel;
		[CustomField("fax")]
		public string fax;
		[CustomField("email")]
		public string email;
		[CustomField("startDate")]
		public DateTime? startDate;
		[CustomField("status")]
		public int? status;
		[CustomField("salary")]
		public double? salary;
		[CustomField("salaryUnit")]
		public string salaryUnit;
		[CustomField("emplCost")]
		public double? emplCost;
		[CustomField("empCostUnt")]
		public string empCostUnt;
		[CustomField("termDate")]
		public DateTime? termDate;
		[CustomField("termReason")]
		public int? termReason;
		[CustomField("bankCode")]
		public string bankCode;
		[CustomField("bankBranch")]
		public string bankBranch;
		[CustomField("bankBranNo")]
		public string bankBranNo;
		[CustomField("bankAcount")]
		public string bankAcount;
		[CustomField("homeStreet")]
		public string homeStreet;
		[CustomField("homeBlock")]
		public string homeBlock;
		[CustomField("homeZip")]
		public string homeZip;
		[CustomField("homeCity")]
		public string homeCity;
		[CustomField("homeCounty")]
		public string homeCounty;
		[CustomField("homeCountr")]
		public string homeCountr;
		[CustomField("homeState")]
		public string homeState;
		[CustomField("birthDate")]
		public DateTime? birthDate;
		[CustomField("brthCountr")]
		public string brthCountr;
		[CustomField("martStatus")]
		public string martStatus;
		[CustomField("nChildren")]
		public int? nChildren;
		[CustomField("govID")]
		public string govID;
		[CustomField("citizenshp")]
		public string citizenshp;
		[CustomField("passportNo")]
		public string passportNo;
		[CustomField("passportEx")]
		public DateTime? passportEx;
		[CustomField("picture")]
		public string picture;
		[CustomField("remark")]
		public string remark;
		[CustomField("attachment")]
		public string attachment;
		[CustomField("salaryCurr")]
		public string salaryCurr;
		[CustomField("empCostCur")]
		public string empCostCur;
		[CustomField("WorkBuild")]
		public string WorkBuild;
		[CustomField("HomeBuild")]
		public string HomeBuild;
		[CustomField("position")]
		public int? position;
		[CustomField("AtcEntry")]
		public int? AtcEntry;
		[CustomField("AddrTypeW")]
		public string AddrTypeW;
		[CustomField("AddrTypeH")]
		public string AddrTypeH;
		[CustomField("StreetNoW")]
		public string StreetNoW;
		[CustomField("StreetNoH")]
		public string StreetNoH;
		[CustomField("DispMidNam")]
		public string DispMidNam;
		[CustomField("NamePos")]
		public string NamePos;
		[CustomField("DispComma")]
		public string DispComma;
		[CustomField("CostCenter")]
		public string CostCenter;
		[CustomField("CompanyNum")]
		public string CompanyNum;
		[CustomField("VacPreYear")]
		public int? VacPreYear;
		[CustomField("VacCurYear")]
		public int? VacCurYear;
		[CustomField("MunKey")]
		public string MunKey;
		[CustomField("TaxClass")]
		public string TaxClass;
		[CustomField("InTaxLiabi")]
		public string InTaxLiabi;
		[CustomField("EmTaxCCode")]
		public string EmTaxCCode;
		[CustomField("RelPartner")]
		public string RelPartner;
		[CustomField("ExemptAmnt")]
		public double? ExemptAmnt;
		[CustomField("ExemptUnit")]
		public string ExemptUnit;
		[CustomField("AddiAmnt")]
		public double? AddiAmnt;
		[CustomField("AddiUnit")]
		public string AddiUnit;
		[CustomField("TaxOName")]
		public string TaxOName;
		[CustomField("TaxONum")]
		public string TaxONum;
		[CustomField("HeaInsName")]
		public string HeaInsName;
		[CustomField("HeaInsCode")]
		public string HeaInsCode;
		[CustomField("HeaInsType")]
		public string HeaInsType;
		[CustomField("SInsurNum")]
		public string SInsurNum;
		[CustomField("StatusOfP")]
		public string StatusOfP;
		[CustomField("StatusOfE")]
		public string StatusOfE;
		[CustomField("BCodeDateV")]
		public string BCodeDateV;
		[CustomField("DevBAOwner")]
		public string DevBAOwner;
		[CustomField("FNameSP")]
		public string FNameSP;
		[CustomField("SurnameSP")]
		public string SurnameSP;
		[CustomField("LogInstanc")]
		public int? LogInstanc;
		[CustomField("UserSign")]
		public int? UserSign;
		[CustomField("UserSign2")]
		public int? UserSign2;
		[CustomField("UpdateDate")]
		public DateTime? UpdateDate;
		[CustomField("PersGroup")]
		public string PersGroup;
		[CustomField("JTCode")]
		public string JTCode;
		[CustomField("ExtEmpNo")]
		public string ExtEmpNo;
		[CustomField("BirthPlace")]
		public string BirthPlace;
		[CustomField("PymMeth")]
		public string PymMeth;
		[CustomField("ExemptCurr")]
		public string ExemptCurr;
		[CustomField("AddiCurr")]
		public string AddiCurr;
		[CustomField("STDCode")]
		public int? STDCode;
		[CustomField("FatherName")]
		public string FatherName;
		[CustomField("CPF")]
		public string CPF;
		[CustomField("CRC")]
		public string CRC;
		[CustomField("ContResp")]
		public string ContResp;
		[CustomField("RepLegal")]
		public string RepLegal;
		[CustomField("DirfDeclar")]
		public string DirfDeclar;
		[CustomField("UF_CRC")]
		public string UF_CRC;
		[CustomField("IDType")]
		public string IDType;
		[CustomField("Active")]
		public string Active;
		[CustomField("BPLId")]
		public int? BPLId;
		[CustomField("ManualNUM")]
		public string ManualNUM;
		[CustomField("PassIssue")]
		public DateTime? PassIssue;
		[CustomField("PassIssuer")]
		public string PassIssuer;
		[CustomField("QualCode")]
		public string QualCode;
		[CustomField("PRWebAccss")]
		public string PRWebAccss;
		[CustomField("PrePRWeb")]
		public string PrePRWeb;
		[CustomField("U_MSTCOD")]
		public string U_MSTCOD;
		[CustomField("U_JICCOD")]
		public string U_JICCOD;
		[CustomField("U_GNTEXE")]
		public string U_GNTEXE;
		[CustomField("U_KUKJUN")]
		public double? U_KUKJUN;
		[CustomField("U_RETDAT")]
		public DateTime? U_RETDAT;
		[CustomField("U_BOJDAT")]
		public DateTime? U_BOJDAT;
		[CustomField("U_SUJDAT")]
		public DateTime? U_SUJDAT;
		[CustomField("U_BALYMD")]
		public DateTime? U_BALYMD;
		[CustomField("U_BUSTYP")]
		public string U_BUSTYP;
		[CustomField("U_PRJCOD")]
		public string U_PRJCOD;
		[CustomField("U_CSTCOD")]
		public string U_CSTCOD;
		[CustomField("U_PNLCOD")]
		public string U_PNLCOD;
		[CustomField("U_DIM3CD")]
		public string U_DIM3CD;
		[CustomField("U_NOJGBN")]
		public string U_NOJGBN;
		[CustomField("U_GRPDAT")]
		public DateTime? U_GRPDAT;
		[CustomField("U_BIRGBN")]
		public string U_BIRGBN;
		[CustomField("U_BSPCOD")]
		public string U_BSPCOD;
	}
}
