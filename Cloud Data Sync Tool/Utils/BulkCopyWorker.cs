using System;
using System.ComponentModel;
using CloudSync.Models;

namespace CloudSync.Utils
{
    public class BulkCopyWorker
    {
        private readonly BackgroundWorker _worker;
        private static volatile BulkCopyWorker _instance;
        private static readonly object Lock = new object();

        public bool IsWorking => _worker.IsBusy;

        public static BulkCopyWorker Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Lock)
                    {
                        if (_instance == null)
                            _instance = new BulkCopyWorker();
                    }
                }

                return _instance;
            }
        }

        private BulkCopyWorker()
        {
            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _worker.DoWork += DoWork;
        }

        public void InitWorker(RunWorkerCompletedEventHandler completedHandler, ProgressChangedEventHandler reportHandler)
        {
            _worker.RunWorkerCompleted += completedHandler;
            _worker.ProgressChanged += reportHandler;
        }

        public void StartWorker(WorkerArgs args)
        {
            if (!IsWorking)
                _worker.RunWorkerAsync(args);
        }

        public void CancelWorker()
        {
            if(IsWorking)
                _worker.CancelAsync();
        }

        private static void DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var args = (WorkerArgs)e.Argument;
            var dbUtil = new DbUtils(args.SrcConnection, args.DstConnection);

            //var stopWatch = new Stopwatch();
            double progress = 0, step = (double) 100 / args.SrcTables.Count;
            var copiedTables = 0;

            foreach (var table in args.SrcTables)
            {
                if (worker == null || worker.CancellationPending)
                {
                    e.Cancel = true;
                    //stopWatch.Stop();
                    worker?.ReportProgress((int)Math.Ceiling(progress).Clamp(0, 100), $@"Copying table {table} is cancelled.");
                    break;
                }

                //stopWatch.Restart();
                worker.ReportProgress((int) Math.Ceiling(progress).Clamp(0, 100), $@"Copying table {table}.");
                var count = dbUtil.BulkCopy(args.SrcSchemaName, args.DstSchemaName, table);
                progress += step;

                copiedTables++;
            }

            if(copiedTables == args.SrcTables.Count)
                worker?.ReportProgress((int)Math.Ceiling(progress).Clamp(0, 100), @"Copy completed.");
            
            e.Result = copiedTables;
        }
    }
}
