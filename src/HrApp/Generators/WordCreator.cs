﻿using HrApp.Contracts;
using HrApp.Entities;
using HrApp.Repositories;
using SautinSoft.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HrApp
{
    public class WordCreator : IDocumentCreator
    {
        public IFileRepository FileRepo { get; set; }

        public async Task GenerateAbsenceWordAsync(EmployeeEntity employee, string reason, AbsenceRequestEntity absenceRequest, string end, string absenceType, string days)
        {
            string fileName = @"" + employee.FirstName[0] + "." + employee.LastName + " "
                + String.Format("{0:MM-dd}", absenceRequest.AbsenceStart) + " iki " + String.Format("{0:MM-dd}", absenceRequest.AbsenceEnd) + ".docx";

            var document = FormatWordDocument(DateTime.Now, DateTime.Now, employee, reason, end, absenceType, days);

            //saving to memory stream, to give ability delete sautinsoft autogenerated text.
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms, new DocxSaveOptions());
                ms.Seek(0, SeekOrigin.Begin);
                document = DocumentCore.Load(ms, new DocxLoadOptions());
            }

            var doc = DeleteWaterMark(document);
            var key = await FileRepo.UploadFile(fileName, doc, absenceRequest.Id);
        }

        public void GenerateEmployeeReportWord(EmployeeEntity employee, DateTime startDate, DateTime endDate)
        {
            string fileName = @"" + employee.FirstName + "_" + employee.LastName + "_"
                + String.Format("{0:d}", startDate) + "_"
                + String.Format("{0:d}", endDate) + "_report.docx";

            var document = FormatEmployeeReportWord(employee, startDate, endDate);

            //saving to memory stream, to give ability delete sautinsoft autogenerated text.
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms, new DocxSaveOptions());
                ms.Seek(0, SeekOrigin.Begin);
                document = DocumentCore.Load(ms, new DocxLoadOptions());
            }

            DeleteWaterMark(document);

            document.Save(fileName, new DocxSaveOptions());
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fileName) { UseShellExecute = true });

            //var key = await FileRepo.UploadFile(fileName, doc);
        }

        public void GenerateSelectedProjectReportWord(ProjectEntity project, DateTime startDate, DateTime endDate)
        {
            string fileName = @"" + project.Name + "_"
                + String.Format("{0:d}", startDate) + "_"
                + String.Format("{0:d}", endDate) + "_report.docx";

            var document = FormatProjectReportWord(project, startDate, endDate);

            //saving to memory stream, to give ability delete sautinsoft autogenerated text.
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms, new DocxSaveOptions());
                ms.Seek(0, SeekOrigin.Begin);
                document = DocumentCore.Load(ms, new DocxLoadOptions());
            }
            DeleteWaterMark(document);

            document.Save(fileName, new DocxSaveOptions());
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fileName) { UseShellExecute = true });

            //var key = await FileRepo.UploadFile(fileName, doc);
        }

        public void GenerateProjectsReportWord(List<ProjectEntity> projects, DateTime startDate, DateTime endDate)
        {
            string fileName = @"" + String.Format("{0:d}", startDate) + "_"
                            + String.Format("{0:d}", endDate) + "_projects_report.docx";

            var document = FormatProjectsReportWord(projects, startDate, endDate);

            //saving to memory stream, to give ability delete sautinsoft autogenerated text.
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms, new DocxSaveOptions());
                ms.Seek(0, SeekOrigin.Begin);
                document = DocumentCore.Load(ms, new DocxLoadOptions());
            }

            DeleteWaterMark(document);

            document.Save(fileName, new DocxSaveOptions());
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fileName) { UseShellExecute = true });

            //var key = await FileRepo.UploadFile(fileName, doc);
        }

        private async Task<CommitEntity> GetCommit(string id)
        {
            var repo = new CommitRepository();

            var commit = await repo.GetCommitById(id);

            return commit;
        }

        private async Task<EmployeeEntity> GetEmployee(string id)
        {
            var repo = new EmployeesRepository();

            var emp = await repo.GetEmployeeById(id);

            return emp;
        }

        private DocumentCore FormatProjectReportWord(ProjectEntity project, DateTime startDate, DateTime endDate)
        {
            DocumentCore docx = new DocumentCore();
            var titleFormat = new CharacterFormat() { FontName = "Times New Roman", FontColor = Color.Black, Size = 14, Bold = true };
            var textFormat = new CharacterFormat() { FontName = "Times New Roman", Size = 12, FontColor = Color.Black, Bold = false };

            Section section = new Section(docx);
            docx.Sections.Add(section);

            //company title
            //--------------------------------------------------
            Paragraph paragraph1 = new Paragraph(docx);
            paragraph1.ParagraphFormat.Alignment = HorizontalAlignment.Center;
            section.Blocks.Add(paragraph1);

            Run company = new Run(docx, "UAB \"Present Connection\"");
            company.CharacterFormat = titleFormat;
            paragraph1.Inlines.Add(company);
            paragraph1.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            paragraph1.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

            //--------------------------------------------------

            //summary 
            //--------------------------------------------------
            Paragraph paragraph3 = new Paragraph(docx);
            paragraph3.ParagraphFormat.Alignment = HorizontalAlignment.Left;
            section.Blocks.Add(paragraph3);

            Run dateTitle = new Run(docx, "Date: ");
            dateTitle.CharacterFormat = titleFormat.Clone();
            paragraph3.Inlines.Add(dateTitle);

            Run date = new Run(docx, String.Format("{0:yyy.MM.dd}", startDate) + " - " + String.Format("{0:yyy.MM.dd}", endDate));
            date.CharacterFormat = textFormat.Clone();
            paragraph3.Inlines.Add(date);
            paragraph3.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            //--------------------------------------------------

            //document title
            //--------------------------------------------------
            Paragraph paragraph2 = new Paragraph(docx);
            paragraph2.ParagraphFormat.Alignment = HorizontalAlignment.Center;
            section.Blocks.Add(paragraph2);

            Run docTitle = new Run(docx, "Work time report");
            docTitle.CharacterFormat = titleFormat.Clone();
            paragraph2.Inlines.Add(docTitle);
            paragraph2.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            //--------------------------------------------------

            //report 
            //--------------------------------------------------
            Paragraph paragraph4 = new Paragraph(docx);
            paragraph4.ParagraphFormat.Alignment = HorizontalAlignment.Left;
            section.Blocks.Add(paragraph4);

            Run proName = new Run(docx, "Project:");
            proName.CharacterFormat = titleFormat.Clone();
            paragraph4.Inlines.Add(proName);

            proName = new Run(docx, project.Name);
            proName.CharacterFormat = textFormat.Clone();
            paragraph4.Inlines.Add(proName);

            paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));


            proName = new Run(docx, "Employee".PadRight(30));
            proName.CharacterFormat = titleFormat.Clone();
            paragraph4.Inlines.Add(proName);

            proName = new Run(docx, "Time worked".PadRight(30));
            proName.CharacterFormat = titleFormat.Clone();
            paragraph4.Inlines.Add(proName);

            paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

            foreach (var empId in project.Employees)
            {
                double empWorkTime = 0;
                empWorkTime = CalculateEmployeeTime(empId, project.Commits, startDate, endDate);
                var employee = GetEmployee(empId).Result;

                Run emp = new Run(docx, (employee.FirstName + " " + employee.LastName).PadRight(50));
                emp.CharacterFormat = textFormat.Clone();
                paragraph4.Inlines.Add(emp);

                emp = new Run(docx, empWorkTime.ToString());
                emp.CharacterFormat = textFormat.Clone();
                paragraph4.Inlines.Add(emp);


                paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
                paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

            }
            //--------------------------------------------------
            return docx;
        }

        private DocumentCore FormatProjectsReportWord(List<ProjectEntity> projects, DateTime startDate, DateTime endDate)
        {
            DocumentCore docx = new DocumentCore();
            var titleFormat = new CharacterFormat() { FontName = "Times New Roman", FontColor = Color.Black, Size = 14, Bold = true };
            var textFormat = new CharacterFormat() { FontName = "Times New Roman", Size = 12, FontColor = Color.Black, Bold = false };

            Section section = new Section(docx);
            docx.Sections.Add(section);

            //company title
            //--------------------------------------------------
            Paragraph paragraph1 = new Paragraph(docx);
            paragraph1.ParagraphFormat.Alignment = HorizontalAlignment.Center;
            section.Blocks.Add(paragraph1);

            Run company = new Run(docx, "UAB \"Present Connection\"");
            company.CharacterFormat = titleFormat;
            paragraph1.Inlines.Add(company);
            paragraph1.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            paragraph1.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

            //--------------------------------------------------

            //summary 
            //--------------------------------------------------
            Paragraph paragraph3 = new Paragraph(docx);
            paragraph3.ParagraphFormat.Alignment = HorizontalAlignment.Left;
            section.Blocks.Add(paragraph3);

            Run dateTitle = new Run(docx, "Date: ");
            dateTitle.CharacterFormat = titleFormat.Clone();
            paragraph3.Inlines.Add(dateTitle);

            Run date = new Run(docx, String.Format("{0:yyy.MM.dd}", startDate) + " - " + String.Format("{0:yyy.MM.dd}", endDate));
            date.CharacterFormat = textFormat.Clone();
            paragraph3.Inlines.Add(date);
            paragraph3.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            //--------------------------------------------------

            //document title
            //--------------------------------------------------
            Paragraph paragraph2 = new Paragraph(docx);
            paragraph2.ParagraphFormat.Alignment = HorizontalAlignment.Center;
            section.Blocks.Add(paragraph2);

            Run docTitle = new Run(docx, "Work time report");
            docTitle.CharacterFormat = titleFormat.Clone();
            paragraph2.Inlines.Add(docTitle);
            paragraph2.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            //--------------------------------------------------

            //report 
            //--------------------------------------------------
            Paragraph paragraph4 = new Paragraph(docx);
            paragraph4.ParagraphFormat.Alignment = HorizontalAlignment.Left;
            section.Blocks.Add(paragraph4);
            foreach (var project in projects)
            {
                Run proName = new Run(docx, "Project:");
                proName.CharacterFormat = titleFormat.Clone();
                paragraph4.Inlines.Add(proName);


                proName = new Run(docx, project.Name);
                proName.CharacterFormat = textFormat.Clone();
                paragraph4.Inlines.Add(proName);

                paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
                paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));


                proName = new Run(docx, "Employee".PadRight(30));
                proName.CharacterFormat = titleFormat.Clone();
                paragraph4.Inlines.Add(proName);


                proName = new Run(docx, "Time worked".PadRight(30));
                proName.CharacterFormat = titleFormat.Clone();
                paragraph4.Inlines.Add(proName);
                paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

                foreach (var empId in project.Employees)
                {
                    double empWorkTime = 0;
                    empWorkTime = CalculateEmployeeTime(empId, project.Commits, startDate, endDate);
                    var employee = GetEmployee(empId).Result;

                    string n = employee.FirstName + " " + employee.LastName;
                    Run emp = new Run(docx, n.PadRight(50));
                    emp.CharacterFormat = textFormat.Clone();
                    paragraph4.Inlines.Add(emp);

                    emp = new Run(docx, empWorkTime.ToString());
                    emp.CharacterFormat = textFormat.Clone();
                    paragraph4.Inlines.Add(emp);


                    paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
                    paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

                }
            }
            //--------------------------------------------------

            return docx;
        }

        private double CalculateEmployeeTime(string employeeId, List<string> commits, DateTime startDate, DateTime endDate)
        {
            double time = 0;
            foreach (var commitId in commits)
            {
                var commit = GetCommit(commitId).Result;
                if (commit.Employee == employeeId
                    && commit.CommitDate >= startDate
                    && commit.CommitDate <= endDate)
                {
                    time += commit.TimeWorked;
                }
            }
            return time;
        }

        private DocumentCore FormatEmployeeReportWord(EmployeeEntity employee, DateTime startDate, DateTime endDate)
        {
            var repo = new ProjectRepository();
            var projects = repo.GetAllProjects().Result;

            DocumentCore docx = new DocumentCore();
            var titleFormat = new CharacterFormat() { FontName = "Times New Roman", FontColor = Color.Black, Size = 14, Bold = true };
            var textFormat = new CharacterFormat() { FontName = "Times New Roman", Size = 12, FontColor = Color.Black, Bold = false };

            Section section = new Section(docx);
            docx.Sections.Add(section);

            //company title
            //--------------------------------------------------
            Paragraph paragraph1 = new Paragraph(docx);
            paragraph1.ParagraphFormat.Alignment = HorizontalAlignment.Center;
            section.Blocks.Add(paragraph1);

            Run company = new Run(docx, "UAB \"Present Connection\"");
            company.CharacterFormat = titleFormat;
            paragraph1.Inlines.Add(company);
            paragraph1.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            paragraph1.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

            //--------------------------------------------------

            //summary 
            //--------------------------------------------------
            Paragraph paragraph3 = new Paragraph(docx);
            paragraph3.ParagraphFormat.Alignment = HorizontalAlignment.Left;
            section.Blocks.Add(paragraph3);

            Run dateTitle = new Run(docx, "Date: ");
            dateTitle.CharacterFormat = titleFormat.Clone();
            paragraph3.Inlines.Add(dateTitle);

            Run date = new Run(docx, String.Format("{0:yyy.MM.dd}", startDate) + " - " + String.Format("{0:yyy.MM.dd}", endDate));
            date.CharacterFormat = textFormat.Clone();
            paragraph3.Inlines.Add(date);
            paragraph3.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

            Run empTitle = new Run(docx, "Employee: ");
            empTitle.CharacterFormat = titleFormat.Clone();
            paragraph3.Inlines.Add(empTitle);

            Run emp = new Run(docx, employee.FirstName + " " + employee.LastName);
            emp.CharacterFormat = textFormat.Clone();
            paragraph3.Inlines.Add(emp);
            paragraph3.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            //--------------------------------------------------

            //document title
            //--------------------------------------------------
            Paragraph paragraph2 = new Paragraph(docx);
            paragraph2.ParagraphFormat.Alignment = HorizontalAlignment.Center;
            section.Blocks.Add(paragraph2);

            Run docTitle = new Run(docx, "Work time report");
            docTitle.CharacterFormat = titleFormat.Clone();
            paragraph2.Inlines.Add(docTitle);
            paragraph2.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            //--------------------------------------------------

            //report 
            //--------------------------------------------------
            Paragraph paragraph4 = new Paragraph(docx);
            paragraph4.ParagraphFormat.Alignment = HorizontalAlignment.Left;
            section.Blocks.Add(paragraph4);

            double totalWorkTime = 0;
            foreach (var project in projects)
            {
                if (project.Employees.Contains(employee.Id))
                {
                    Run projName = new Run(docx, "Project name:");
                    projName.CharacterFormat = titleFormat.Clone();
                    paragraph4.Inlines.Add(projName);

                    projName = new Run(docx, project.Name);
                    projName.CharacterFormat = textFormat.Clone();
                    paragraph4.Inlines.Add(projName);
                    paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

                    Run proName = new Run(docx, "Employee".PadRight(20));
                    proName.CharacterFormat = titleFormat.Clone();
                    paragraph4.Inlines.Add(proName);

                    proName = new Run(docx, "Time worked".PadRight(30));
                    proName.CharacterFormat = titleFormat.Clone();
                    paragraph4.Inlines.Add(proName);
                    paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

                    paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
                    foreach (var id in project.Commits)
                    {
                        var commit = GetCommit(id).Result;
                        if (commit.Employee == employee.Id)
                        {
                            if (commit.CommitDate >= startDate && commit.CommitDate <= endDate)
                            {
                                totalWorkTime += commit.TimeWorked;

                                Run comm = new Run(docx, commit.Description);
                                comm.CharacterFormat = textFormat.Clone();
                                paragraph4.Inlines.Add(comm);
                                paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.Tab));
                                paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.Tab));

                                comm = new Run(docx, commit.TimeWorked.ToString());
                                comm.CharacterFormat = textFormat.Clone();
                                paragraph4.Inlines.Add(comm);
                                paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
                            }
                        }
                    }
                }
                paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
                paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            }

            Paragraph paragraph5 = new Paragraph(docx);
            paragraph5.ParagraphFormat.Alignment = HorizontalAlignment.Left;
            section.Blocks.Add(paragraph5);

            Run time = new Run(docx, "Total employee work time: ");
            time.CharacterFormat = titleFormat.Clone();
            paragraph5.Inlines.Add(time);
            paragraph4.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.Tab));

            time = new Run(docx, totalWorkTime.ToString());
            time.CharacterFormat = textFormat.Clone();
            paragraph5.Inlines.Add(time);
            paragraph5.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));

            //--------------------------------------------------
            return docx;
        }

        private DocumentCore FormatWordDocument(DateTime dateFrom, DateTime dateTo, EmployeeEntity employee, string reason, string end, string absenceType, string days)
        {
            DocumentCore docx = new DocumentCore();

            Section section = new Section(docx);
            docx.Sections.Add(section);

            Paragraph paragraph1 = new Paragraph(docx);
            paragraph1.ParagraphFormat.Alignment = HorizontalAlignment.Center;
            section.Blocks.Add(paragraph1);

            var cFormat = new CharacterFormat() { FontName = "Times New Roman", FontColor = Color.Black, Size = 12f, Bold = true };

            Run nameSurname = new Run(docx, employee.FirstName + " " + employee.LastName + "");
            nameSurname.CharacterFormat = cFormat;
            paragraph1.Inlines.Add(nameSurname);
            paragraph1.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            Run identity = nameSurname.Clone();
            identity.Text = employee.PersonalId.ToString();
            paragraph1.Inlines.Add(identity);
            paragraph1.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            Run address = nameSurname.Clone();
            address.Text = "gyvenantis " + employee.Address;
            paragraph1.Inlines.Add(address);

            SpecialCharacter lBr = new SpecialCharacter(docx, SpecialCharacterType.LineBreak);
            docx.Content.End.Insert(lBr.Content);
            docx.Content.End.Insert(lBr.Content);
            docx.Content.End.Insert(lBr.Content);

            Paragraph paragraph2 = new Paragraph(docx);
            paragraph2.ParagraphFormat.Alignment = HorizontalAlignment.Left;
            section.Blocks.Add(paragraph2);
            var cFormat2 = new CharacterFormat() { FontName = "Times New Roman", Size = 12, FontColor = Color.Black, Bold = false };
            Run company = new Run(docx, "UAB \"Present Connection\"");
            company.CharacterFormat = cFormat2;
            paragraph2.Inlines.Add(company);
            paragraph2.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            Run whom = company.Clone();
            whom.Text = "Direktoriui ";
            paragraph2.Inlines.Add(whom);
            paragraph2.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            Run director = company.Clone();
            director.Text = "Domantui Jovaišui";
            paragraph2.Inlines.Add(director);
            docx.Content.End.Insert(lBr.Content);
            docx.Content.End.Insert(lBr.Content);
            docx.Content.End.Insert(lBr.Content);

            Paragraph paragraph3 = new Paragraph(docx);
            paragraph3.ParagraphFormat.Alignment = HorizontalAlignment.Center;
            section.Blocks.Add(paragraph3);
            var cFormat3 = new CharacterFormat() { FontName = "Times New Roman", Size = 12, FontColor = Color.Black, Bold = true };
            Run reque = new Run(docx, "PRAŠYMAS");
            reque.CharacterFormat = cFormat3;
            paragraph3.Inlines.Add(reque);
            paragraph3.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            Run why = reque.Clone();
            why.Text = "DEL " + reason;
            paragraph3.Inlines.Add(why);
            paragraph3.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            Run date = reque.Clone();
            DateTime dt = DateTime.Now;

            date.Text = String.Format("{0:yyyy-MM-dd}", dt);
            paragraph3.Inlines.Add(date);
            paragraph3.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            Run city = reque.Clone();
            city.Text = "Kaunas";
            paragraph3.Inlines.Add(city);
            docx.Content.End.Insert(lBr.Content);
            docx.Content.End.Insert(lBr.Content);
            docx.Content.End.Insert(lBr.Content);

            Paragraph paragraph4 = new Paragraph(docx);
            paragraph4.ParagraphFormat.Alignment = HorizontalAlignment.Left;
            section.Blocks.Add(paragraph4);
            var cFormat4 = new CharacterFormat() { FontName = "Times New Roman", Size = 12, FontColor = Color.Black, Bold = false };
            Run text = new Run(docx, "Prašau suteikti man " + days + ", " + absenceType + ", nuo " + String.Format("{0:yyyy-MM-dd}", dateFrom) + " iki "
                + String.Format("{0:yyyy-MM-dd}", dateTo) + ". " + end + ".");
            text.CharacterFormat = cFormat4;
            paragraph4.Inlines.Add(text);

            for (int i = 0; i < 20; i++)
            {
                docx.Content.End.Insert(lBr.Content);
            }

            Paragraph paragraph5 = new Paragraph(docx);
            paragraph5.ParagraphFormat.Alignment = HorizontalAlignment.Right;
            section.Blocks.Add(paragraph5);
            var cFormat5 = new CharacterFormat() { FontName = "Times New Roman", Size = 12, FontColor = Color.Black, Bold = false };
            Run nSurname = new Run(docx, employee.FirstName[0] + "." + employee.LastName);
            nSurname.CharacterFormat = cFormat5;
            paragraph5.Inlines.Add(nSurname);

            return docx;

        }

        private DocumentCore DeleteWaterMark(DocumentCore dc)
        {
            string textToDelete = "Created by unlicensed version of Document .Net 4.2.1.24!";
            string textToDelete2 = "The unlicensed version sometimes inserts " + '"' + "trial" + '"' + " into random places.";
            string textToDelete3 = "This text will disappear after purchasing the license.";
            string textToDelete4 = "This text will disappear trial purchasing the license.";
            string textToDelete5 = "trial ";
            //  DocumentCore dc = DocumentCore.Load(filePath);
            dc = DeleteCurrentSelectionLine(dc, textToDelete);
            dc = DeleteCurrentSelectionLine(dc, textToDelete2);
            dc = DeleteCurrentSelectionLine(dc, textToDelete3);
            dc = DeleteCurrentSelectionLine(dc, textToDelete4);
            dc = DeleteCurrentSelectionLine(dc, textToDelete5);
            return dc;
        }

        private DocumentCore DeleteCurrentSelectionLine(DocumentCore dc, string textToDelete)
        {
            int countDel = 0;
            foreach (ContentRange cr in dc.Content.Find(textToDelete).Reverse())
            {
                cr.Delete();
                countDel++;
            }
            return dc;
        }

        public void GenerateEmployeeReportExcel(EmployeeEntity employee, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public void GenerateProjectReportExcel(ProjectEntity project, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public void GenerateMultipleProjectsReportExcel(List<ProjectEntity> projects, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }
    }
}
