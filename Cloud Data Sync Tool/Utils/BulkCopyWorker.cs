using System.ComponentModel;
using System.Diagnostics;
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

            var stopWatch = new Stopwatch();
            int progress = 0, copiedTables = 0;

            foreach (var table in args.SrcTables)
            {
                if (worker == null || worker.CancellationPending)
                {
                    e.Cancel = true;
                    stopWatch.Stop();
                    break;
                }

                stopWatch.Restart();
                var dumpFile = dbUtil.SaveTable(args.SrcSchemaName, table, args.DumpDirectory);
                progress += 50 / args.SrcTables.Count;
                worker.ReportProgress(progress, $@"Table '{args.SrcSchemaName}.{table}' saved. Elapsed time: {stopWatch.Elapsed.TotalMilliseconds / 1000:0.####}s");
                
                stopWatch.Restart();
                var count = dbUtil.BulkLoad(args.SrcSchemaName, args.DstSchemaName, table, dumpFile, args.DeleteFile);
                progress += 50 / args.SrcTables.Count;
                worker.ReportProgress(progress, $@"Table '{args.DstSchemaName}.{table}' loaded. Count: {count}, Elapsed time: {stopWatch.Elapsed.TotalMilliseconds / 1000:0.####}s");

                copiedTables++;
            }

            e.Result = copiedTables;
        }
    }
}
