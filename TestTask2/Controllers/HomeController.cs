﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using TestTask2.Models;
using Excel = Microsoft.Office.Interop.Excel;

namespace TestTask2.Controllers
{
    public class HomeController : Controller
    {
        private TestDBEntities db = new TestDBEntities();
        public ActionResult Index()
        {
            return View(db.DataFiles.ToList());
        }
        [HttpPost]
        public ActionResult Index(MyFiles file)
        {
            foreach (var mfile in file.Files)
            {
                if (mfile == null)
                {
                    ViewBag.FileEror = "please Select file";
                    return View(db.DataFiles.ToList());
                }
                if (mfile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(mfile.FileName);
                    if(db.DataFiles.SingleOrDefault(fi => fi.FileName == fileName)==null){
                        DataFile f = new DataFile();
                        f.FileName = fileName;
                        db.DataFiles.Add(f);
                        db.SaveChanges();
                        var path = Path.Combine(Server.MapPath("~/Content/files"), fileName);
                        mfile.SaveAs(path);
                    }
                }
                //ViewBag.fileName = fileName;
            }
            return RedirectToAction("Index");
        }
        public ActionResult ShowDataFromFile(int idF)
        {
            DataFile f = db.DataFiles.Find(idF);
            return RedirectToAction("DocData");
        }
        public ActionResult GetDataFromFile(int idF)
        {
            DataFile f = db.DataFiles.Find(idF);
            if(f.Uploaded) return RedirectToAction("DocData");
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;


            int rCnt;
            int cCnt;
            int rw = 0;
            //int cl = 0;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(Path.Combine(Server.MapPath("~/Content/files"), f.FileName), 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            range = xlWorkSheet.UsedRange;
            rw = range.Rows.Count;
            //cl = range.Columns.Count;
            string sClass = "";
            for (rCnt = 1; rCnt <= rw; rCnt++)
            {
                Sheet obj = null;
                OpeningBalance oBal = null;
                ClosingBalance cBal = null;
                Circulation circ = null;
                for (cCnt = 1; cCnt <= 7; cCnt++)
                {
                    if (range.Cells[rCnt, cCnt] != null && range.Cells[rCnt, cCnt].Value2 != null)
                    {
                        Type type = range.Cells[rCnt, cCnt].Value2.GetType();
                        if (type == typeof(string))
                        {
                            string val = range.Cells[rCnt, cCnt].Value2;
                            int id;
                            if (cCnt == 1 && int.TryParse(val, out id))
                            {
                                obj = new Sheet();
                                obj.Id = id;
                                obj.DataFile = f;
                            }
                            if (val.Contains("КЛАСС "))
                            {
                                sClass = val;
                            }
                            else continue;
                        }
                        if (type == typeof(double) && obj != null)
                        {
                            if (cCnt == 2)
                            {
                                oBal = new OpeningBalance();
                                oBal.Asset = range.Cells[rCnt, cCnt].Value2;
                            }
                            if (cCnt == 3 && oBal != null)
                            {
                                oBal.Liability = range.Cells[rCnt, cCnt].Value2;
                                oBal.Sheet = obj;
                                db.OpeningBalancies.Add(oBal);
                            }
                            if (cCnt == 4)
                            {
                                circ = new Circulation();
                                circ.Debit = range.Cells[rCnt, cCnt].Value2;
                            }
                            if (cCnt == 5 && circ != null)
                            {
                                circ.Credit = range.Cells[rCnt, cCnt].Value2;
                                circ.Sheet = obj;
                                db.Circulations.Add(circ);
                            }
                            if (cCnt == 6)
                            {
                                cBal = new ClosingBalance();
                                cBal.Asset = range.Cells[rCnt, cCnt].Value2;
                            }
                            if (cCnt == 7 && cBal != null)
                            {
                                cBal.Liability = range.Cells[rCnt, cCnt].Value2;
                                cBal.Sheet = obj;
                                db.ClosingBalancies.Add(cBal);
                            }
                            //Console.Write(range.Cells[rCnt, cCnt].Value2.ToString() + "\t");
                        }
                        if (cCnt == 7 && obj != null)
                        {
                            obj.Class = sClass;
                            db.Sheets.Add(obj);
                            db.SaveChanges();
                        }
                    }
                }

            }
            f.Uploaded = true;
            db.Entry(f).State = EntityState.Modified;
            db.SaveChanges();
            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);
            return RedirectToAction("Index");
        }
        public ActionResult DocData()
        {
            return View(db.Sheets.ToList());
        }
        public ActionResult DeleteFile(int idF)
        {
            DataFile f = db.DataFiles.Find(idF);
            db.DataFiles.Remove(f);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}