﻿namespace EA.Weee.Web.Areas.AatfReturn.ViewModels
{
    using EA.Weee.Core.AatfReturn;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public abstract class SelectReportOptionsModelBase
    {
        protected SelectReportOptionsModelBase()
        {
        }

        public virtual Guid OrganisationId { get; set; }

        public virtual Guid ReturnId { get; set; }

        public virtual IList<ReportOnQuestion> ReportOnQuestions { get; set; }

        public virtual bool HasSelectedOptions => ReportOnQuestions != null && ReportOnQuestions.Any(c => c.Selected);

        public virtual List<int> DeSelectedOptions
        {
            get
            {
                return ReportOnQuestions.Where(r => r.DeSelected).Select(r => r.Id).ToList();
            }
        }

        public virtual List<int> SelectedOptions
        {
            get
            {
                return ReportOnQuestions.Where(r => r.Selected && r != DcfQuestion).Select(r => r.Id).ToList();
            }
        }

        private string dcfSelectedValue;
        public virtual string DcfSelectedValue
        {
            get
            {
                if (!NonObligatedQuestionSelected)
                {
                    DcfSelectedValue = null;
                }

                return dcfSelectedValue;
            }
            set
            {
                dcfSelectedValue = value;

                if (DcfQuestion != null)
                {
                    DcfQuestion.Selected = value?.Equals(YesValue) ?? false;
                }
            }
        }

        public virtual ReportOnQuestion DcfQuestion
        {
            get
            {
                return ReportOnQuestions.FirstOrDefault(d => d.Id.Equals((int)ReportOnQuestionEnum.NonObligatedDcf));
            }
        }

        public virtual bool DcfQuestionSelected
        {
            get
            {
                return ReportOnQuestions.Any(r => r.Id.Equals((int)ReportOnQuestionEnum.NonObligatedDcf) && r.Selected);
            }
        }

        public virtual bool NonObligatedQuestionSelected
        {
            get
            {
                return ReportOnQuestions.Any(r => r.Id.Equals((int)ReportOnQuestionEnum.NonObligated) && r.Selected);
            }
        }

        public IList<string> DcfPossibleValues => new List<string> { "Yes", "No" };

        public string YesValue => DcfPossibleValues.ElementAt(0);

        public string NoValue => DcfPossibleValues.ElementAt(1);
    }
}