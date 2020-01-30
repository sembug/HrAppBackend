﻿using CodeMash.Client;
using CodeMash.Project.Services;
using Isidos.CodeMash.ServiceContracts;
using SautinSoft.Document;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HrApp
{
    public class WordCreator
    {
        private static CodeMashClient Client => new CodeMashClient(Settings.ApiKey, Settings.ProjectId);
        public IFileRepository FileRepo { get; set; }
       // public IVacationRepository VacationRepo { get; set; }
        public async Task GenerateWordAsync(DateTime dateFrom, DateTime dateTo, EmployeeEntity employee, string template)
        {

            string fileName = @"" + employee.FirstName[0] + "." + employee.LastName + " "
                + String.Format("{0:MM-dd}", dateFrom) + " iki " + String.Format("{0:MM-dd}", dateTo) + ".docx";

            var document = FormatWordDocument(fileName, dateFrom, dateTo, employee, template);

            
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms, new DocxSaveOptions()); 
                ms.Seek(0, SeekOrigin.Begin);
                document = DocumentCore.Load(ms, new DocxLoadOptions());
            }
                        
            var doc = DeleteWaterMark(document);      
           
            var key = await FileRepo.UploadFile(fileName, doc);

           // await VacationRepo.InsertVacationRequest(dateFrom, dateTo, employee, key);
                       
        }


        private DocumentCore FormatWordDocument(string filePath, DateTime dateFrom, DateTime dateTo, EmployeeEntity employee, string template)
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
            identity.Text = "Identity";
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
            Run company = new Run(docx, "UAB ,,Present Connection``");
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
            why.Text = "DĖL KASMETINIŲ ATOSTOGŲ ";
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
            Run text = new Run(docx, "Prašau suteikti man " + template + " nuo " + String.Format("{0:yyyy-MM-dd}", dateFrom) + " iki "
                + String.Format("{0:yyyy-MM-dd}", dateTo) + "  pinigus išmokant kartu su atlyginimu.");
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
    }
}
