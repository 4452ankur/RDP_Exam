using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestRdP;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Xml.Serialization;
using System.Globalization;



namespace TestRdP
{
    public class Utility
    {
        internal static void ProcessAllMethod()  //St --7.47 Pm ,End--11.30 pm
        {
            string TxtFldrPath = @"C:\Users\Ankur\source\repos\TestRdP\TestRdP\Read";
            string TxtFileNm = @"RawFile.txt";
            string XmlFileNm = @"RawFile.xml";
            string Constr = @"Password=P@ssw0rd;Persist Security Info=True;User ID=sa;Initial Catalog=RDP;Data Source=ANKUR-PC\SQLEXPRESS";
            string TxtDestFldrPth = @"C:\Users\Ankur\source\repos\TestRdP\TestRdP\Write";
            string TxtOldFileName = @"RawFile_ankur.txt";
            string XmlOldFileName = @"RawFile_ankur.xml";

            DataTable Dt = new DataTable();
            List<Property> LstRw,LstValid = new List<Property>();
            LstRw = ReadTxtFile(TxtFldrPath, TxtFileNm);
            LstValid = ValidChk(LstRw);
            InsertRawDatabase(Constr,LstRw);
            Dt = GetData(Constr);
            UpdateCalData(Constr, LstValid, Dt);
            WriteTxtFile(TxtDestFldrPth, TxtOldFileName, LstRw);
            WriteXmlFile(TxtDestFldrPth, XmlOldFileName, LstRw);
            LstRw = ReadRawXmlFile(TxtFldrPath, XmlFileNm);
            
        }

        private static List<Property> ReadRawXmlFile(string txtFldrPath, string xmlFileNm)//st:10.14 end:10.27
        {
            List<Property> LstRaw = new List<Property>();
            string FullPath = Path.Combine(txtFldrPath, xmlFileNm);
            using (FileStream FS = new FileStream(FullPath, FileMode.Open, FileAccess.Read))
            {
               
                    XmlSerializer XS=new XmlSerializer(typeof(List<Property>));
                    LstRaw= (List < Property >) XS.Deserialize(FS);
            
            }
            return LstRaw;
        }

        private static void WriteXmlFile(string txtDestFldrPth, string xmlOldFileName, List<Property> lstRw) //st: 10.06 End : 10.12
        {
            string FileNm = DateTime.Now.ToString("MMddyyyy");
            string NewFileName = xmlOldFileName.Replace(xmlOldFileName.Substring(8, 5), FileNm);
            string NewPath = Path.Combine(txtDestFldrPth, NewFileName);
            string ArchivePth = Path.Combine(txtDestFldrPth, "Archive");
            string ArchiveFullPth = Path.Combine(ArchivePth, NewFileName);

            if (File.Exists(NewPath))
            {
                File.Delete(NewPath);
            }
            using (FileStream FS = new FileStream(NewPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                //using (StreamWriter Sw = new StreamWriter(FS))
                //{
                    XmlSerializer Xs = new XmlSerializer(typeof(List<Property>));
                    Xs.Serialize(FS, lstRw);
               // }
               
            }
            if (File.Exists(ArchiveFullPth))
            {
                File.Delete(ArchiveFullPth);
                File.Copy(NewPath, ArchiveFullPth);
            }
            else
            {
                File.Copy(NewPath, ArchiveFullPth); ;
            }
        }

        private static void WriteTxtFile(string txtDestFldrPth, string txtOldFileName, List<Property> lstRw)//st:9.43p.m. End:10.01
        {
            string FileNm = DateTime.Now.ToString("MMddyyyy");
            string NewFileName = txtOldFileName.Replace(txtOldFileName.Substring(8, 5), FileNm);
            string NewPath = Path.Combine(txtDestFldrPth, NewFileName);
            string ArchivePth = Path.Combine(txtDestFldrPth, "Archive");
            string ArchiveFullPth = Path.Combine(ArchivePth, NewFileName);
            if (File.Exists(NewPath))
            {
                File.Delete(NewPath);
            }
            using (FileStream FS = new FileStream(NewPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                
                using (StreamWriter Sw = new StreamWriter(FS))
                {
                    foreach (var item in lstRw)
                    {
                        Sw.WriteLine(item.RawProdId + "," + item.ModelId + "," + item.ModelDate + "," + item.ProdDate + "," + item.ModelPrice);
                    }                   
                }
               if( File.Exists(ArchiveFullPth))
                {
                    File.Delete(ArchiveFullPth);
                    File.Copy(NewPath, ArchiveFullPth);
                }
                else
                {
                    File.Copy(NewPath, ArchiveFullPth); 
                }
            }
        }

        private static void UpdateCalData(string constr, List<Property> lstValid, DataTable dt)//st:9.16 p.m. end:9.36 p.m.
        {
            try
            {
                var lstItem = from lst in lstValid
                              where lst.Flag == "Valid"
                              select lst;
                foreach (var item in lstItem)
                {
                    int prodId = item.ProdId;
                    string modelid = item.ModelId;
                    int modePrice = item.ModelPrice;
                    foreach (DataRow dr in dt.Rows)
                    {
                        string Tblmodelid = dr["model_id"].ToString();
                        if(modelid== Tblmodelid)
                        {
                            int model_amount= Convert.ToInt32( dr["model_amount"]);
                            int calData = model_amount * modePrice;

                            using (SqlConnection SqlCon = new SqlConnection(constr))
                            {
                                SqlCon.Open();
                                string Query = "Update Product_Details SET model_sale_price = @calData  Where model_id = @model_id AND prod_id = @prod_id";
                                using (SqlCommand Sqlcomm = new SqlCommand(Query, SqlCon))
                                {
                                    Sqlcomm.Parameters.Clear();
                                    Sqlcomm.Parameters.AddWithValue("@calData", calData);
                                    Sqlcomm.Parameters.AddWithValue("@model_id", modelid);
                                    Sqlcomm.Parameters.AddWithValue("@prod_id", prodId);
                                    Sqlcomm.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
              
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateCalData () ---- " + ex.ToString());
            }           

        }

        private static DataTable GetData(string constr) // St: 9.08 p.m. end:9.13 p.m.
        {
            DataTable dt = new DataTable();
            try
            {
                SqlConnection SqlCon = new SqlConnection(constr);
                string Query = "SELECT  [model_id],[model_amount] FROM [Model_Details]";
                SqlCommand SqlCmd = new SqlCommand(Query, SqlCon);
                SqlDataAdapter SqlAdp = new SqlDataAdapter(SqlCmd);
               
                SqlAdp.Fill(dt);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetData() ---- "+ ex.ToString());
            }
            return dt;

        }

        private static void InsertRawDatabase(string Constr,List<Property> lstRw) // st : 8.51 p.m. end:9.05 p.m.
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(Constr))
                {
                    SqlCon.Open();
                    string Query = "Insert Into Product_Details (prod_id,model_id,prod_date,model_date,ModlPrice) Values (@prod_id,@model_id,@prod_date,@model_date,@ModlPrice)";
                    using (SqlCommand SqlCmd = new SqlCommand(Query, SqlCon))
                    {
                        foreach (var item in lstRw)
                        {
                            try
                            {
                                SqlCmd.Parameters.Clear();
                                SqlCmd.Parameters.AddWithValue("@prod_id", item.RawProdId);
                                SqlCmd.Parameters.AddWithValue("@model_id", item.ModelId);
                                SqlCmd.Parameters.AddWithValue("@prod_date", item.ProdDate);
                                SqlCmd.Parameters.AddWithValue("@model_date", item.ModelDate);
                                SqlCmd.Parameters.AddWithValue("@ModlPrice", item.ModelPrice);
                                SqlCmd.ExecuteNonQuery();

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("InsertRawDatabase() ---- " + ex.ToString());
                                continue;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("InsertRawDatabase1() ---- " + ex.ToString());
            }
       
            
        }

        private static List<Property> ValidChk(List<Property> lstRw) //st:8.14p.m. end 8.49 p.m.
        {
            int OutInt = 0;
            DateTime OutMdldt = DateTime.Now;
            DateTime OutProddt = DateTime.Now;
            string DateFormat = "MM/dd/yyyy";
            bool flg = true;
            List<Property> LstChkd = new List<Property>();
            foreach (var item in lstRw)
            {
                Property Prop = new Property();
                try
                {
                    flg = int.TryParse(item.RawProdId,out OutInt);
                    if(flg==true && item.ModelId.StartsWith("ML"))
                    {
                        flg = true;
                    }
                    else
                    {
                        flg = false;
                    }
                    if (flg == true)
                    {
                        flg = int.TryParse(item.ModelId.Substring(2, 3), out OutInt);
                    }
                        if (flg == true)
                        {
                        flg = DateTime.TryParseExact(item.ModelDate.ToString(), DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out OutMdldt);
                        }
                    if (flg == true)
                    {
                        flg = DateTime.TryParseExact(item.ProdDate.ToString(), DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out OutProddt);
                    }
                    if(OutMdldt.AddDays(10)> OutProddt)
                    {
                        flg = false;
                    }
                    if(flg==true)
                    {
                        Prop.Flag = "Valid";
                    }
                    else
                    {
                        Prop.Flag = "In Valid";
                    }
                  
                    Prop.ProdId = int.Parse(item.RawProdId);
                    Prop.ModelId = Convert.ToString(item.ModelId);
                    Prop.ModelDate = Convert.ToString(item.ModelDate);
                    Prop.ProdDate = Convert.ToString(item.ProdDate);
                    Prop.ModelPrice = Convert.ToInt32(item.ModelPrice);
                    LstChkd.Add(Prop);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    continue;
                }
            }
            return LstChkd;

        }

        private static List<Property> ReadTxtFile(string txtFldrPath, string txtFileNm) //St --7.47 Pm ,End--8.12 pm
        {
            List<Property> LstRaw = new List<Property>();
            try
            {
              
                string FullPath = Path.Combine(txtFldrPath, txtFileNm);
                using (FileStream FS = new FileStream(FullPath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader SR = new StreamReader(FS))
                    {
                        string AllText = SR.ReadToEnd();
                        string[] AllTxtArray = AllText.Split('\n');
                        for (int i = 1; AllTxtArray.Length>i; i++)
                        {
                            try
                            {
                                Property Prop = new Property();
                                string Line = AllTxtArray[i];
                                if (Line.Length > 1)
                                {
                                    string[] LineArray = Line.Replace("\r".ToString(), "").Split(',');
                                    Prop.RawProdId = LineArray[0];
                                    Prop.ModelId = LineArray[1];
                                    Prop.ModelDate = LineArray[2];
                                    Prop.ProdDate = LineArray[3];
                                    Prop.ModelPrice =Convert.ToInt32( LineArray[4]);
                                    LstRaw.Add(Prop);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ReadTxtFile() ---" + ex.ToString());
                                continue;
                            }                         

                        }

                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            return LstRaw;

        }
    }
}
