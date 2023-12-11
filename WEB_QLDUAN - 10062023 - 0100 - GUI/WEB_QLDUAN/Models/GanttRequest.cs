using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace WEB_QLDUAN.Models
{
    public class GanttRequest
    {
        public GanttMode Mode { get; set; }
        public GanttAction Action { get; set; }
        public Task UpdatedTask { get; set; }
        public Link UpdatedLink { get; set; }
        public Object SourceId { get; set; }

        /// <summary>
        /// Create new GanttData object and populate it
        /// </summary>
        /// <param name="form">Form collection</param>
        /// <returns>New GanttData</returns>
        public static List<GanttRequest> Parse(FormCollection form, string ganttMode)
        {
            // save current culture and change it to InvariantCulture for data parsing
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var dataActions = new List<GanttRequest>();
            var prefixes = form["ids"].Split(',');

            foreach (var prefix in prefixes)
            {
                var request = new GanttRequest();

                // lambda expression for form data parsing
                Func<string, string> parse = x => form[String.Format("{0}_{1}", prefix, x)];

                request.Mode = (GanttMode)Enum.Parse(typeof(GanttMode), ganttMode, true);
                request.Action = (GanttAction)Enum.Parse(typeof(GanttAction), parse("!nativeeditor_status"), true);
                //request.SourceId = Int64.Parse(parse("id"));
                // string a = parse("id");
                request.SourceId = parse("id");

                // parse gantt task
                if (request.Action != GanttAction.Deleted && request.Mode == GanttMode.Tasks)
                {

                    object sort = parse("order");

                    string ID = (request.Action == GanttAction.Updated) ? request.SourceId.ToString() : "0";
                    string TaskName = parse("text");
                    DateTime StartDate = DateTime.Parse(parse("start_date"));
                    int Duration = (parse("duration") != null) ? Int32.Parse(parse("duration")) : 0;
                    double Progress = (parse("progress") != null) ? Convert.ToDouble(parse("progress")) : 0;
                    string ParentID = (parse("parent") != "0") ? parse("parent") : null;
                    int SortOrder = (parse("order") != null && parse("order") != "") ? Int32.Parse(parse("order")) : 0;

                    Task t = new Task();

                    t.ID = (request.Action == GanttAction.Updated) ? request.SourceId.ToString() : "0";
                    t.TaskName = parse("text");

                    if (parse("start_date") != null && parse("start_date") != "")
                        t.StartDate = DateTime.Parse(parse("start_date"));
                    else t.StartDate = null;

                    if (parse("duration") != null && parse("duration") != "")
                        t.Duration = Int32.Parse(parse("duration"));
                    else t.Duration = 0;

                    if (parse("progress") != null && parse("progress") != "")
                        t.Progress = Convert.ToDouble(parse("progress"));
                    else t.Progress = 0;

                    if (parse("parent") != "0")
                        t.ParentID = parse("parent");
                    else t.ParentID = null;

                    if (parse("order") != null && parse("order") != "")
                        t.SortOrder = Int32.Parse(parse("order"));
                    else t.SortOrder = 0;

                    request.UpdatedTask = t;

                    /*
                    request.UpdatedTask = new Task()
                    {
                        ID = (request.Action == GanttAction.Updated) ? request.SourceId.ToString() : "0",
                        TaskName = parse("text"),
                        StartDate = DateTime.Parse(parse("start_date")),
                        Duration = (parse("duration") != null) ? Int32.Parse(parse("duration")) : 0,
                        Progress = (parse("progress") != null) ? Convert.ToDouble(parse("progress")) : 0,
                        ParentID = (parse("parent") != "0") ? parse("parent") : null,
                        SortOrder = (parse("order") != null) ? Int32.Parse(parse("order")) : 0
                        // Type = parse("type")
                    };*/
                }
                // parse gantt link
                else if (request.Action != GanttAction.Deleted && request.Mode == GanttMode.Links)
                {
                    request.UpdatedLink = new Link()
                    {
                        ID = (request.Action == GanttAction.Updated) ? (int)request.SourceId : 0,
                        SourceTaskID = parse("source"),
                        TargetTaskID = parse("target"),
                        Type = parse("type")
                    };
                }

                dataActions.Add(request);
            }

            // return current culture back
            Thread.CurrentThread.CurrentCulture = currentCulture;

            return dataActions;
        }
    }

    /// <summary>
    /// Gantt modes
    /// </summary>
    public enum GanttMode
    {
        Tasks,
        Links
    }

    /// <summary>
    /// Gantt actions
    /// </summary>
    public enum GanttAction
    {
        Inserted,
        Updated,
        Deleted,
        Error
    }
}