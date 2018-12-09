using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.Xml.Serialization;

namespace RDP_Framework
{
   public class Utility
    {

        public void Process_AllMethods()
        {
            List<Prop_details> LstRaw = new List<Prop_details>();
            LstRaw = lstPropDtls(@"C:\Users\Ankur\source\repos\RDP_Framework\ReadFolder", "RDP_Insert.txt");

            List<Prop_details> lstValid = new List<Prop_details>();
            lstValid = ListValid(LstRaw);
            string constr = @"Data Source =BUBUN-PC\SQLEXPRESS;User ID = sa;Password = p@ssw0rd;Persist Security Info = True;";

            string flWrtpath = @"C:\Users\Ankur\source\repos\RDP_Framework\WriteFolder";

            InsertRawData(constr, LstRaw);
            DataTable dt = new DataTable();
           dt= Getdata(constr);
            InsertCalData(lstValid, dt, constr);
            InsertIntoTxtFile(lstValid, flWrtpath);
            InsertIntoXML(LstRaw, flWrtpath);

            LstRaw.Clear();

            LstRaw = XmlRedLst(@"C:\Users\Ankur\source\repos\RDP_Framework\ReadFolder", "Who_Ankur_Product.xml");

        }

        public List<Prop_details> lstPropDtls(string sourcepath,string filename)
        {
            List<Prop_details> lstProp = new List<Prop_details>();
            try
            {
                string filepath;// = sourcepath + "//" + filename;
                filepath = Path.Combine(sourcepath, filename);
                FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                using (StreamReader sr = new StreamReader(fs))
                {
                    string alltext = sr.ReadToEnd();
                    string[] textArray = alltext.Split('\n');
                    string ddtFormat = ("MM/dd/yyyy");
                    for(int i=0; textArray.Length>i;i++)
                    {
                        string line = textArray[i];
                        string[] lineArray = line.Replace("\r".ToString(), "").Split(',');
                        Prop_details prop_Details = new Prop_details();
                        prop_Details.ProdId = Convert.ToInt32(lineArray[0]);
                        prop_Details.Model_Id = Convert.ToString( lineArray[1]);

                     //   prop_Details.Prod_date = DateTime.ParseExact(lineArray[2], ddtFormat,CultureInfo.InvariantCulture);
                     //   prop_Details.Model_date = DateTime.ParseExact(lineArray[3], ddtFormat, CultureInfo.InvariantCulture);

                        prop_Details.Prod_date = lineArray[2];
                        prop_Details.Model_date = lineArray[3];

                        prop_Details.Model_Price = Convert.ToInt32(lineArray[4]);
                        
                        lstProp.Add(prop_Details);
                    }

                    return lstProp;
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

                return lstProp;
            }
            
        }

        public List<Prop_details>ListValid(List<Prop_details> lst)
        {
            bool flg = true;
            string modelId;
            int resultProd, result;
            string dtformat = "MM/dd/yyyy";
            DateTime dtoutProd, dtoutModel;
          
            var lstValid = new List<Prop_details>(lst);
            lstValid.Clear();


            foreach (var item in lst)
            {

                Prop_details prop_Details = new Prop_details();

                flg = int.TryParse(item.ProdId.ToString(), out resultProd);
                modelId = item.Model_Id;
                if(modelId.Substring(0,2).ToString()!="ML")
                {
                    flg = false;
                }
                else
                {
                    flg = int.TryParse(modelId.Substring(2, 3), out result);
                }

                string dt = item.Prod_date;
                flg = DateTime.TryParseExact(dt, dtformat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtoutProd);

                flg = DateTime.TryParseExact(item.Model_date, dtformat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtoutModel);
                if (dtoutModel>=dtoutProd.AddDays(10))
                {
                    flg = true;
                }
                else
                {
                    flg = false;
                }

                prop_Details.ProdId = item.ProdId;
                prop_Details.Model_Id = item.Model_Id;
                prop_Details.Prod_date = item.Prod_date;
                prop_Details.Model_date = item.Model_date;
                prop_Details.Model_Price = item.Model_Price;
                if (flg==true)
                {

                    prop_Details.Flag = "V";
                    
                }
                else
                {
                    prop_Details.Flag = "E";
                }
                lstValid.Add(prop_Details);
            }

            return lstValid;
        }

        public void  InsertRawData(string constr,List<Prop_details> lst)
        {
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(constr))
                {
                    sqlConn.Open();
                    string InsertQuery = "Insert into [Rdp].[dbo].[prdo_details] (prod_id,model_id,prod_date,model_date,model_price) values (@prod_id,@model_id,@prod_date,@model_date,@model_price)";
                    using (SqlCommand comm = new SqlCommand(InsertQuery, sqlConn))
                    {
                        foreach (var item in lst)
                        {
                            comm.Parameters.Clear();
                            comm.Parameters.AddWithValue("@prod_id", item.ProdId);
                            comm.Parameters.AddWithValue("@model_id", item.Model_Id);
                            comm.Parameters.AddWithValue("@prod_date", item.Prod_date);
                            comm.Parameters.AddWithValue("@model_date", item.Model_date);
                            comm.Parameters.AddWithValue("@model_price", item.Model_Price);
                            comm.ExecuteNonQuery();


                        }

                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }       

        }

        public static DataTable Getdata(string constr)
        {
            DataTable dt = new DataTable();
            try {   
            
            using (SqlConnection SqlConn = new SqlConnection(constr))
            {
                SqlConn.Open();
                string SqlQuery = "SELECT  [Model_id] ,[model_amount]  FROM[Rdp].[dbo].[model]";
                using (SqlCommand sqlCmd = new SqlCommand(SqlQuery, SqlConn))
                {
                    SqlDataAdapter sqlAdp = new SqlDataAdapter(sqlCmd);
                        sqlAdp.Fill(dt);
                    }
            }              

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return dt;
        }

        public void InsertCalData(List<Prop_details>lst,DataTable dt,string constr)
        {
            var newLst = from s in lst
                         where s.Flag == "V"
                         select s;

            foreach(var item in newLst)
            {
                string LstmodId = item.Model_Id;
                foreach(DataRow dr in dt.Rows)
                {
                    string DtmodId = dr["Model_id"].ToString();
                    if(LstmodId== DtmodId)
                    {
                        try
                        {
                            SqlConnection sqlCon = new SqlConnection(constr);
                            sqlCon.Open();
                            string sqlcmdTxt = "Insert into [Rdp].[dbo].[model_details] (prod_id,modelId,modelByCost) values(@prod_id,@modelId,@modelByCost)";
                            int modelSalCos = item.Model_Price * Convert.ToInt32(dr["model_amount"].ToString());
                            using (SqlCommand sqlCmd = new SqlCommand(sqlcmdTxt, sqlCon))
                            {
                                sqlCmd.Parameters.AddWithValue("@prod_id", item.ProdId);
                                sqlCmd.Parameters.AddWithValue("@modelId", item.Model_Id);
                                sqlCmd.Parameters.AddWithValue("@modelByCost", modelSalCos);
                                sqlCmd.ExecuteNonQuery();
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }

        }

        public void InsertIntoTxtFile(List<Prop_details> lst,string writepath)
        {
            var lstInvalid = from s in lst
                             where s.Flag == "E"
                             select s;
            string fileName = "ankur_product.txt";
            string newFileNm = DateTime.Now.ToString("MMddyyyy").ToString();
         //   fileName = (fileName.Substring(0, 5));
          string Nwfilname=  fileName.Replace(fileName.Substring(0, 5), newFileNm);
            string Nwpath = Path.Combine(writepath, Nwfilname);

            FileStream fs = new FileStream(Nwpath, FileMode.OpenOrCreate, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(fs))
            {
               foreach(var item in lstInvalid)
                {
                    sw.WriteLine(item.ProdId + "," + item.Prod_date + item.Model_Id + "," + item.Model_Price + item.Model_date + "," + item.Flag);
                }                
            }      

        }

        public void InsertIntoXML(List<Prop_details>lst,string xmlpath)
        {
            var ListAll = new List<Prop_details>();
            
                var test = from s in lst
                      where s.Model_Price <= 300
                      select s ;
            ListAll.AddRange(test);

            string FilName = "Who_Ankur_Product.xml";
            string dtnam = DateTime.Now.ToString("MMddyyyy");
            string NwFilename = FilName.Replace(FilName.Substring(4,5), dtnam);
            string nwPth = Path.Combine(xmlpath, NwFilename);
            try
            {
                if(File.Exists(nwPth))
                {
                    File.Delete(nwPth);
                }
                 using (FileStream fs = new FileStream(nwPth, FileMode.OpenOrCreate, FileAccess.Write))
                  {
                     XmlSerializer xmlSr = new XmlSerializer(typeof(List<Prop_details>));
                   xmlSr.Serialize(fs, ListAll);
                 }
                if (File.Exists(@"C:\Users\Ankur\source\repos\RDP_Framework\Archive\" + FilName))
                {
                    File.Delete(@"C:\Users\Ankur\source\repos\RDP_Framework\Archive\" + FilName);
                }
                    File.Copy(nwPth, @"C:\Users\Ankur\source\repos\RDP_Framework\Archive\"+ FilName);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public List<Prop_details> XmlRedLst(string path,string filename)
        {
            string fullpath = Path.Combine(path, filename);
            List<Prop_details> lst = new List<Prop_details>();
            using (FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xmlSr = new XmlSerializer(typeof(List<Prop_details>));
                lst = (List<Prop_details>)xmlSr.Deserialize(fs);
                return lst;
            }
        }
    }
}
