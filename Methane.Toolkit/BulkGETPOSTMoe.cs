using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using static Methane.Toolkit.Common;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;

namespace Methane.Toolkit
{
    public class BulkGETPOSTMoe
    {


        string cookie;
        string bl;

        Dictionary<string, string> Names;

        public void PromptParamenters()
        {
        }


        public void Run()
        {

            Log("");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("               Methane                  ");
            Log("         Bulk HTTP GET POST             ");
            Log("  Deigned for http://onlineexams.gov.lk ");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("");




            cookie = "cookie.txt";
            try
            {
                cookie = File.ReadAllText(cookie);
            }
            catch (Exception ex)
            {
                LogError(ex, "Cannot read the Cookie file");
                //return;
                cookie = "no:cookie";
            }



            bl = $"---------------------------51921082225895";



            string SDoc = GetForm("http://onlineexams.gov.lk/onlineapps/index.php/schoolapp/stuoapp");
            string lookFor = "http://onlineexams.gov.lk/onlineapps/index.php/schoolapp/stuoapp/edit/";

            var Indexes = AllIndexesOf(SDoc, lookFor);

            Prompt($"{Indexes.Count} students found. Proceed?");

            Names = new Dictionary<string, string>();

            //int Errors = 0;

            //for (int i = 0; i < Indexes.Count; i++)
            //{
            //    string stdIndex = Indexes[i];
            //    GetStd(stdIndex, out string cand_medium, out string sn, out string cand_name, out string cand_gender, out string cand_dob, out string cand_mobile, out string cand_nic);

            //    bool IsOK = true;
            //    bool IsDupNIC = false;

            //    if (string.IsNullOrEmpty(cand_nic) || string.IsNullOrWhiteSpace(cand_nic))
            //    {
            //        IsOK = false;
            //        // Log($"{stdIndex} name {cand_name} Empty NIC");
            //        Log($"{stdIndex} No NIC   {(cand_medium == "4" ? "E" : "S")}   {cand_name}  @{cand_mobile}");
            //    }
            //    else
            //    {
            //        if (Names.ContainsKey(cand_nic))
            //        {
            //            string DupName = "Not found";

            //            foreach (var item in Names)
            //            {
            //                if (item.Key == cand_nic)
            //                {
            //                    DupName = item.Value;
            //                }
            //            }

            //            //Log($"{stdIndex} NIC {cand_nic} name {cand_name} is duplicate 'NIC' with name {DupName}");
            //            Log($"{stdIndex} NIC      {(cand_medium == "4" ? "E" : "S")}   ({cand_name} {cand_nic}) with ({DupName})  @{cand_mobile}");
            //            IsOK = false;
            //            IsDupNIC = true;
            //            Errors++;
            //        }
            //    }



            //    if (Names.ContainsValue(cand_name))
            //    {

            //        string DupID = "Not found";

            //        foreach (var item in Names)
            //        {
            //            if (item.Value == cand_name)
            //            {
            //                DupID = item.Key;
            //            }
            //        }

            //        //Log($"{stdIndex} NIC {cand_nic} name {cand_name} is duplicate 'Name' with ID {DupID}");

            //        Log($"{stdIndex} NAME     {(cand_medium == "4" ? "E" : "S")}   ({cand_name} {cand_nic}) with ({DupID})  @{cand_mobile}");

            //        IsOK = false;
            //        if (!IsDupNIC) Errors++;
            //    }

            //    // Log(IsOK ? $"{cand_nic} OK" : $"{cand_nic} Error");

            //    if (IsOK)
            //    {
            //        Names.Add(cand_nic, cand_name);
            //    }

            //}

            //Log($"{Names.Count} students OK. {Indexes.Count - Names.Count} students have issues. {Errors} students have Errors");

            for (int i = 0; i < Indexes.Count; i++)
            {
                string stdIndex = Indexes[i];


                string newSn = (i + 1).ToString();

                FixStd(stdIndex, newSn);

            }






        }

        private bool FixStd(string stdIndex, string newSn)
        {
            GetStd(stdIndex, out string cand_medium, out string sn, out string cand_name, out string cand_gender, out string cand_dob, out string cand_mobile, out string cand_nic);

            cand_name = cand_name.ToUpper();

            // if(Names.TryGetValue(cand_nic ,out string cand_nameOld))

            POSTStd(stdIndex, cand_medium, newSn, cand_name, cand_gender, cand_dob, cand_mobile, cand_nic);

            GetStd(stdIndex, out string cand_medium2, out string sn2, out string cand_name2, out string cand_gender2, out string cand_dob2, out string cand_mobile2, out string cand_nic2);

            return sn2 == newSn;

        }

        private void GetStd(string stdIndex, out string cand_medium, out string sn, out string cand_name, out string cand_gender, out string cand_dob, out string cand_mobile, out string cand_nic)
        {
            string URLGET = $"http://onlineexams.gov.lk/onlineapps/index.php/schoolapp/stuoapp/edit/{stdIndex}";

            var doc = GetFormDoc(URLGET, cookie);


            cand_medium = "2";
            string candMedium = doc.GetElementbyId("field-cand_medium").OuterHtml;
            if (candMedium.Contains("selected='selected'"))
            {
                cand_medium = candMedium[candMedium.IndexOf("selected='selected'") - 3].ToString();
            }

            sn = doc.GetElementbyId("field-sn").GetAttributeValue("value", "");
            cand_name = doc.GetElementbyId("field-cand_name").GetAttributeValue("value", "");
            cand_gender = doc.GetElementbyId("field-cand_gender").OuterHtml.Contains("selected='selected'") ? "1" : "0";
            cand_dob = doc.GetElementbyId("field-cand_dob").GetAttributeValue("value", "");
            cand_mobile = doc.GetElementbyId("field-cand_mobile").GetAttributeValue("value", "");
            cand_nic = doc.GetElementbyId("field-cand_nic").GetAttributeValue("value", "");
            //Log($" " +
            //    $"sn {sn} \n" +
            //    $"cand_name {cand_name} \n" +
            //    $"cand_gender {cand_gender} \n" +
            //    $"cand_dob {cand_dob} \n" +
            //    $"cand_medium {cand_medium} \n" +
            //    $"cand_mobile {cand_mobile} \n" +
            //    $"cand_nic {cand_nic} \n" +
            //    $"");
        }



        private void POSTStd(string stdIndex, string cand_medium, string sn, string cand_name, string cand_gender, string cand_dob, string cand_mobile, string cand_nic)
        {
            string URLPOST = $"http://onlineexams.gov.lk/onlineapps/index.php/schoolapp/stuoapp/update/{stdIndex}";
            //string URLPOST = $"http://onlineexams.gov.lk/onlineapps/index.php/schoolapp/stuoapp/update_validation/{stdIndex}";

            //string POSTStr = $"{bl}\nContent-Disposition: form-data; name=\"sn\"\n\n{sn}\n{bl}\nContent-Disposition: form-data; name=\"cand_name\"\n\n{cand_name}\n{bl}\nContent-Disposition: form-data; name=\"cand_gender\"\n\n{cand_gender}\n{bl}\nContent-Disposition: form-data; name=\"cand_nic\"\n\n{cand_nic}\n{bl}\nContent-Disposition: form-data; name=\"cand_dob\"\n\n{cand_dob}\n{bl}\nContent-Disposition: form-data; name=\"cand_medium\"\n\n{cand_medium}\n{bl}\nContent-Disposition: form-data; name=\"cand_mobile\"\n\n{cand_mobile}\n{bl}\nContent-Disposition: form-data; name=\"cand_special_needs\"\n\n0\n{bl}--\n";

            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent(sn), "sn");
            form.Add(new StringContent(cand_name), "cand_name");
            form.Add(new StringContent(cand_gender), "cand_gender");
            form.Add(new StringContent(cand_dob), "cand_dob");
            form.Add(new StringContent(cand_mobile), "cand_mobile");
            form.Add(new StringContent(cand_nic), "cand_nic");
            form.Add(new StringContent(cand_medium), "cand_medium");
            form.Add(new StringContent("0"), "cand_special_needs");


            // Log(form.ReadAsStringAsync().Result);

            string resp = PostFormMultipart(URLPOST, form, stdIndex);

            bool Succes = resp.Contains("\"success\":true,\"insert_primary_key\":true,\"success_message\"");
            Log(Succes ? $"Std OK {stdIndex} - {sn}" : $"Error \t {stdIndex} \t {resp}");


            //Log(resp);
        }






        public string PostForm(string url, string poststring, string stdID)
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "POST";
            wr.ContentType = $"multipart/form-data; boundary={bl}";//"application /x-www-form-urlencoded";
            wr.Headers.Add("X-Requested-With", "XMLHttpRequest");
            wr.Headers.Add(HttpRequestHeader.Cookie, cookie);
            wr.Headers.Add(HttpRequestHeader.Referer, $"http://onlineexams.gov.lk/onlineapps/index.php/schoolapp/stuoapp/edit/{stdID}");


            wr.KeepAlive = true;

            byte[] bytedata = System.Text.Encoding.UTF8.GetBytes(poststring);
            wr.ContentLength = bytedata.Length;

            // Create the stream
            Stream requestStream = wr.GetRequestStream();
            requestStream.Write(bytedata, 0, bytedata.Length);
            requestStream.Close();

            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }

            string resp = sb.ToString();
            return resp;
        }



        public string PostFormMultipart(string url, MultipartFormDataContent form, string stdID)
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "POST";
            wr.ContentType = form.Headers.ContentType.ToString();// $"multipart/form-data; boundary={bl}";//"application /x-www-form-urlencoded";
            wr.Headers.Add("X-Requested-With", "XMLHttpRequest");
            wr.Headers.Add(HttpRequestHeader.Cookie, cookie);
            wr.Headers.Add(HttpRequestHeader.Referer, $"http://onlineexams.gov.lk/onlineapps/index.php/schoolapp/stuoapp/edit/{stdID}");


            wr.KeepAlive = true;

            byte[] bytedata = form.ReadAsByteArrayAsync().Result;
            wr.ContentLength = bytedata.Length;

            // Create the stream
            Stream requestStream = wr.GetRequestStream();
            requestStream.Write(bytedata, 0, bytedata.Length);
            requestStream.Close();


            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }

            string resp = sb.ToString();
            return resp;
        }




        public string GetForm(string url)
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "GET";
            wr.ContentType = "application/x-www-form-urlencoded";

            if (cookie != "") wr.Headers.Add(HttpRequestHeader.Cookie, cookie);


            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        public HtmlDocument GetFormDoc(string url, string cookie = "")
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "GET";
            // wr.ContentType = "multipart/form-data; boundary=---------------------------19609895721194";

            if (cookie != "") wr.Headers.Add(HttpRequestHeader.Cookie, cookie);


            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(responseStream);

            return doc;
        }

        public IEnumerable<string> AllIndexesOfEnum(string str, string value)
        {
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index, StringComparison.CurrentCulture);
                if (index == -1)
                    break;
                yield return str.Substring(index + value.Length, 6);
            }
        }

        public List<string> AllIndexesOf(string str, string value)
        {
            List<string> indexes = new List<string>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(str.Substring(index + value.Length, 6));
            }
        }
    }

}