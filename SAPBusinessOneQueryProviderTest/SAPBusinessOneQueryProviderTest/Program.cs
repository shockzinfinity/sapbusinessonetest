using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace SAPBusinessOneQueryProviderTest
{
	class Program
	{
		static void Main(string[] args)
		{
			DebugMode mode = (DebugMode)Enum.Parse(typeof(DebugMode), ConfigurationManager.AppSettings["DebugMode"]);

			string filterLastNames = "이";
			string country = "KR";

			if (mode == DebugMode.SAP)
			{
				QueryProvider provider = new SAPB1QueryProvider();
				Query<EmployeeInfo> employeeInfos = new Query<EmployeeInfo>(provider);
				// Evaluator 추가로 인해 where statement 상의 상수부분을 변수로 교체가능
				//IQueryable<EmployeeInfo> query = employeeInfos.Where(x => x.sung == filterLastNames);
				//var query1 = employeeInfos.Where(x => x.sung == filterLastNames).Select(e => new { Name = e.sung + e.firstName, eMail = e.email });
				//Console.WriteLine("Query1:\n{0}\n", query1);

				// 이때 Enumerator 가 작동할 것임, 제네레이션 된 쿼리가 실질적으로 날라가는 부분일듯...이미 레코드셋은 do query 로 실행 될것이고, 이미 리턴 받은 상황일 것임
				//var list = query1.ToList();

				//foreach (var item in list)
				//{
				//	//Console.WriteLine("Name: {0} {1}, employeeId : {2}", item.sung, item.firstName, item.empID);
				//	Console.WriteLine("{0}", item);
				//}

				//Console.WriteLine();

				var query2 = employeeInfos.Select(c => new
				{
					Name = c.sung + c.name,
					MobilePhone = new
					{
						Country = c.homeCountr,
						MobileNumber = c.mobile
					}
				})
				.Where(x => x.MobilePhone.Country == country);
				Console.WriteLine("Query2:\n{0}\n", query2);

				var list2 = query2.ToList();
				foreach (var item in list2)
				{
					Console.WriteLine(item);
				}
			}
			else if (mode == DebugMode.General)
			{
				string connectionString = ConfigurationManager.ConnectionStrings["egtronics"].ConnectionString;

				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					QueryProvider provider = new DbQueryProvider(connection);
					Query<OHEM> employeeInfos = new Query<OHEM>(provider);
					//var query1 = employeeInfos.Where(x => x.lastName == filterLastNames).Select(e => new { Name = e.lastName, eMail = e.email });
					//Console.WriteLine("Query1:\n{0}\n", query1);

					//var list1 = query1.ToList();

					//foreach (var item in list1)
					//{
					//	Console.WriteLine("Name: {0}, eMail : {1}", item.Name, item.eMail);
					//}

					var query2 = employeeInfos.Select(c => new
					{
							Name = c.lastName + c.firstName,
							MobilePhone = new
							{
								Country = c.homeCountr,
								MobileNumber = c.mobile
							}
						})
					.Where(x => x.MobilePhone.Country == country);
					Console.WriteLine("Query2:\n{0}\n", query2);

					var list2 = query2.ToList();

					foreach (var item in list2)
					{
						Console.WriteLine(item);
					}
				}
			}

			Console.ReadLine();
		}
	}

	public enum DebugMode
	{
		General = 0,
		SAP = 1
	}
}
