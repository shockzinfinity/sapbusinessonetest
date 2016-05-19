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
			string connectionString = ConfigurationManager.ConnectionStrings["egtronics"].ConnectionString;

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();

				QueryProvider provider = new DbQueryProvider(connection);

				Query<OHEM> ohems = new Query<OHEM>(provider);

				IQueryable<OHEM> query = ohems.Where(x => x.lastName == "이");

				Console.WriteLine("Query:\n{0}\n", query);

				var list = query.ToList(); // 이때 Enumerator 가 작동할 것임, 제네레이션 된 쿼리가 실질적으로 날라가는 부분일듯...이미 레코드셋은 do query 로 실행 될것이고, 이미 리턴 받은 상황일 것임

				foreach (var item in list)
				{
					Console.WriteLine("Name: {0} {1}, employeeId : {2}", item.lastName, item.firstName, item.empID);
				}
			}

			Console.ReadLine();
		}
	}
}
