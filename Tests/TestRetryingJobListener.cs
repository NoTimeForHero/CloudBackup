﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudBackuper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Config;
using NLog.Targets;
using Quartz;

namespace Tests
{
    [TestClass]
    public class TestRetryingJobListener
    {
        protected IScheduler scheduler;

        [TestInitialize]
        public async Task Startup()
        {
            scheduler = await Initializer.GetScheduler(new Config());
        }

        [TestMethod]
        public async Task TestListener()
        {
            int errorCurrent = 0;
            int errorMax = 3;

            AutoResetEvent waiter = new AutoResetEvent(false);
            var listener = new JobFailureHandler(errorMax, 200);
            listener.onError += err => errorCurrent++;
            listener.retriesFailed += () => waiter.Set();
            scheduler.ListenerManager.AddJobListener(listener);

            var job = JobBuilder.Create<TestJob>().WithIdentity("job1").Build();
            var trigger = TriggerBuilder.Create().WithIdentity("job1").StartNow().Build();

            await scheduler.ScheduleJob(job, trigger);

            waiter.WaitOne();
            Assert.AreEqual(errorMax, errorCurrent);
        }

        [PersistJobDataAfterExecution]
        protected class TestJob : IJob
        {
            public Task Execute(IJobExecutionContext context) => throw new Exception("Test exception!");
        }
    }
}
