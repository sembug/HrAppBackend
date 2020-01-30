﻿using CodeMash.Client;
using CodeMash.Project.Services;
using Isidos.CodeMash.ServiceContracts;
using SautinSoft.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HrApp
{
    public class FileRepository : IFileRepository
    {
        private static CodeMashClient Client => new CodeMashClient(Settings.ApiKey, Settings.ProjectId);
        public Task GetFileId()
        {
            throw new NotImplementedException();
        }

        public async Task<string> UploadFile(string fileName, DocumentCore doc)
        {
            var filesService = new CodeMashFilesService(Client);
            string key;

            using (MemoryStream ms = new MemoryStream())
            {

                doc.Save(ms, new DocxSaveOptions());

                ms.Seek(0, SeekOrigin.Begin);

                byte[] byteArray = new byte[ms.Length];
                int count = ms.Read(byteArray, 0, 20);
                while (count < ms.Length)
                {
                    byteArray[count++] =
                        Convert.ToByte(ms.ReadByte());
                }                            
                    var response = await filesService.UploadRecordFileAsync(byteArray, fileName,
                        new UploadRecordFileRequest
                        {
                            UniqueFieldName = "request",
                            CollectionName = "vacationrequest"
                        }
                    );
                    key = response.Key;
                
            }



           
            
            return key;
        }
    }
}
