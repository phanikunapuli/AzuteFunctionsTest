using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace TestFunctionApp
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            string namefromdatabase = "";
            // parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;


            var str = System.Configuration.ConfigurationManager.ConnectionStrings["SQLConnectionString"].ConnectionString;


            log.Info($"{str} conn");
            //// var str1 ="Server=tcp:testazuresqlserverphani.database.windows.net,1433;Initial Catalog=mySampleDatabase;Persist Security Info=False;User ID={Phani};Password={Password$};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";
            //using (SqlConnection conn = new SqlConnection(builder.connectionstring))
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(str))
            {
                conn.Open();
                log.Info("connection open");

                using (SqlCommand cmd = new SqlCommand("usp_setname", conn))

                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    SqlParameter paramname = new SqlParameter("name", System.Data.SqlDbType.VarChar);
                    paramname.Direction = System.Data.ParameterDirection.Input;
                    paramname.Value = name;
                    cmd.Parameters.Add(paramname);
                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.Info($"{rows} rows were updated");
                }

                using (SqlCommand cmd = new SqlCommand("usp_getname", conn))

                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    SqlParameter paramname = new SqlParameter("name", System.Data.SqlDbType.VarChar);
                    paramname.Direction = System.Data.ParameterDirection.Input;
                    paramname.Value = name;
                    cmd.Parameters.Add(paramname);
                    log.Info($"{name} name");
                    // Execute the command and log the # rows affected.
                    SqlDataReader reader = null;
                    reader = await cmd.ExecuteReaderAsync();
                    bool norows = reader.HasRows;
                    log.Info($"{norows} number");

                    while (reader.Read())
                    {
                        namefromdatabase = reader.GetString(0);

                    }


                    reader.Close();
                    conn.Close();
                }

            }



            if (name == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                name = data?.name;
            }

            return name == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "Hello " + namefromdatabase);
        }
    }
}
