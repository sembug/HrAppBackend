﻿using HrApp;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests
{
    public class HRTests
    {
        IHrService hrService;
        INoobRepository noobform;
        IAbsenceRepository absenceRepository;
        IEmployeesRepository employeesRepository;
        INotificationSender notificationSender;
        IFileRepository fileRepository;

        [SetUp]
        public void Setup()
        {
            noobform = new NoobRepository();
            absenceRepository = new AbsenceRepository();
            employeesRepository = new EmployeesRepository();
            notificationSender = new NotificationSender();
            fileRepository = new FileRepository();
            hrService = new HrService()
            {
                AbsenceRequestRepository= absenceRepository,
                EmployeesRepository = employeesRepository,
                NoobRepository = noobform,
                NotificationSender = notificationSender,
                FileRepository = fileRepository
            };
        }

        [Test]
        public async Task Process_Noob_Form_test()
        {
         await hrService.ProcessNoobForm("5e6b5c118c031800013df1bc");
        }


        [Test]
        public async Task SignatureTests()
        {
            await hrService.GenerateFileWithSignatureAndInsert("5ea16b56b162c90001dae747");
        }
        [Test]
        public async Task Taxonomy()
        {
            await absenceRepository.GetAbsenceByIdWithNames("5ea16b56b162c90001dae747");
        }

    }
}