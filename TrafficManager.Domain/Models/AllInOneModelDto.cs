using System;

namespace TrafficManager.Domain.Models
{
    public class AllInOneModelDto : IotHubModelBaseDto
    {
        public AllInOneModelDto() { }

        public AllInOneModelDto(AllInOneModel fullModel)
        {
            sn = fullModel.EventStream;
            ts = fullModel.Timestamp;
            id = fullModel.DeviceId;
            dt = fullModel.DeviceType;
            iid = fullModel.IntersectionId;
            fn = fullModel.Function;
            dsc = fullModel.Description;
            cs = fullModel.CurrentState;
            pid = fullModel.ParentDeviceId;
            err = fullModel.IsError;
            msg = fullModel.Message;
            os = fullModel.OldState;
            ns = fullModel.NewState;
            u1 = fullModel.UsageFactorOne;
            u2 = fullModel.UsageFactorTwo;
        }

        public Guid? id { get; set; }
        public string dt { get; set; }
        public Guid? iid { get; set; }
        public string fn { get; set; }
        public string dsc { get; set; }
        public string cs { get; set; }
        public Guid? pid { get; set; }
        public bool err { get; set; }
        public string msg { get; set; }
        public string os { get; set; }
        public string ns { get; set; }
        public decimal u1 { get; set; }
        public decimal u2 { get; set; }

        public override IotHubModelBase ToFullModel()
        {
            return new AllInOneModel
            {
                EventStream = sn,
                Timestamp = ts,
                DeviceId = id,
                DeviceType = dt,
                IntersectionId = iid,
                Function = fn,
                Description = dsc,
                CurrentState = cs,
                ParentDeviceId = pid,
                IsError = err,
                Message = msg,
                OldState = os,
                NewState = ns,
                UsageFactorOne = u1,
                UsageFactorTwo = u2
            };
        }
    }
}