﻿
(function () {
    gantt.plugins({
        drag_timeline: true
    });

    gantt.message({
        text: "Use mouse drag and drop to scroll the timeline",
        expire: 15000
    })

    var gantt_control = document.querySelector(".gantt_control");
    gantt_control.addEventListener("change", function (e) {


        updIcon(e.target);

        //var enabled = enableTimelineDnd.checked;
        //var requireCtrlKey = ctrlDragTimeline.checked;

        var enabled = true;
        var requireCtrlKey = false;

        if (!enabled) {
            gantt.config.drag_timeline = null;
        } else {
            if (requireCtrlKey) {
                gantt.config.drag_timeline = {
                    ignore: "",
                    useKey: "ctrlKey"
                };
            } else {
                gantt.config.drag_timeline = {
                    ignore: ".gantt_task_line, .gantt_task_link",
                    useKey: false
                };
            }
        }

    });
    gantt.config.drag_timeline = {
        ignore: ".gantt_task_line, .gantt_task_link",
        useKey: false
    };

    function updIcon(el) {
        el.parentElement.classList.toggle("checked_label");

        var iconEl = el.parentElement.querySelector("i"),
            checked = "check_box",
            unchecked = "check_box_outline_blank",
            className = "icon_color";

        iconEl.textContent = iconEl.textContent == checked ? unchecked : checked;
        iconEl.classList.toggle(className);
    }
    gantt.config.date_format = "%Y-%m-%d %H:%i:%s";

    gantt.templates.grid_row_class = function (start, end, task) {
        var css = [];
        if (gantt.hasChild(task.id)) {
            css.push("folder_row");
        }

        if (task.$virtual) {
            css.push("group_row")
        }

        return css.join(" ");
    };

    gantt.templates.task_row_class = function (start, end, task) {
        return "";
    };

    gantt.templates.timeline_cell_class = function (task, date) {
        if (!gantt.isWorkTime({ date: date, task: task }))
            return "week_end";
        return "";
    };

    gantt.templates.resource_cell_class = function (start_date, end_date, resource, tasks) {
        var css = [];
        css.push("resource_marker");
        if (tasks.length <= 1) {
            css.push("workday_ok");
        } else {
            css.push("workday_over");
        }
        return css.join(" ");
    };

    gantt.templates.resource_cell_value = function (start_date, end_date, resource, tasks) {
        var result = 0;
        tasks.forEach(function (item) {
            var assignments = gantt.getResourceAssignments(resource.id, item.id);
            assignments.forEach(function (assignment) {
                var task = gantt.getTask(assignment.task_id);
                if (resource.type == "work") {
                    result += assignment.value * 1;
                } else {
                    result += assignment.value / (task.duration || 1);
                }
            });
        });

        if (result % 1) {
            result = Math.round(result * 10) / 10;
        }
        return "<div>" + result + "</div>";
    };
    var resourceTemplates = {
        grid_row_class: function (start, end, resource) {
            var css = [];
            if (gantt.$resourcesStore.hasChild(resource.id)) {
                css.push("folder_row");
                css.push("group_row");
            }

            return css.join(" ");
        },
        task_row_class: function (start, end, resource) {
            var css = [];

            if (gantt.$resourcesStore.hasChild(resource.id)) {
                css.push("group_row");
            }

            return css.join(" ");

        }
    };

    gantt.locale.labels.section_resources = "Resources";
    gantt.config.lightbox.sections = [
        { name: "description", height: 38, map_to: "text", type: "textarea", focus: true },
        { name: "resources", type: "resources", map_to: "resources", options: gantt.serverList("people") },
        { name: "time", type: "duration", map_to: "auto" }
    ];

    var resourceConfig = {
        scale_height: 30,
        scales: [
            { unit: "day", step: 1, date: "%d %M" }
        ],
        columns: [
            {
                name: "name", label: "Name", tree: true, width: 250, template: function (resource) {
                    return resource.text;
                }, resize: true
            },
            {
                name: "allocated", label: "Allocated", align: "left", width: 100, template: function (resource) {
                    var assignments = gantt.getResourceAssignments(resource.id);
                    var result = 0;
                    assignments.forEach(function (assignment) {
                        var task = gantt.getTask(assignment.task_id);
                        if (resource.type == "work") {
                            result += task.duration * assignment.value;
                        } else {
                            result += assignment.value * 1;
                        }
                    });

                    if (resource.type == "work") {
                        result = "<b>" + result + "</b> hours";
                    } else {
                        result = "<b>" + result + "</b> " + resource.unit;
                    }

                    return result;
                }, resize: true
            }
        ]
    };

    gantt.config.scales = [
        { unit: "month", step: 1, format: "%F, %Y" },
        { unit: "day", step: 1, format: "%d %M" }
    ];

    gantt.config.auto_scheduling = true;
    gantt.config.auto_scheduling_strict = true;
    gantt.config.work_time = true;
    gantt.config.columns = [
        { name: "text", tree: true, width: 320, resize: true },
        { name: "start_date", align: "center", width: 80, resize: true },
        {
            name: "resources", align: "center", width: 80, label: "Resources", resize: true,
            template: function (task) {
                if (task.type == gantt.config.types.project) {
                    return "";
                }

                var result = "";
                var store = gantt.getDatastore("resource");
                var assignments = task[gantt.config.resource_property];

                if (!assignments || !assignments.length) {
                    return "";
                }

                if (assignments.length == 1) {
                    return store.getItem(assignments[0].resource_id).text.split(",")[0];
                }

                assignments.forEach(function (assignment) {
                    var resource = store.getItem(assignment.resource_id);
                    if (!resource)
                        return;
                    result += "<div class='owner-label' title='" + resource.text + "'>" + resource.text.substr(0, 1) + "</div>";

                });

                return result;
            }
        },
        { name: "duration", width: 60, align: "center", resize: true },
        { name: "add", width: 44 }
    ];

    gantt.config.resource_store = "resource";
    gantt.config.resource_property = "resources";
    gantt.config.order_branch = true;
    gantt.config.open_tree_initially = true;
    gantt.config.scale_height = 50;
    gantt.config.layout = {
        css: "gantt_container",
        rows: [
            {
                gravity: 2,
                cols: [
                    { view: "grid", group: "grids", scrollY: "scrollVer" },
                    { resizer: true, width: 1 },
                    { view: "timeline", scrollX: "scrollHor", scrollY: "scrollVer" },
                    { view: "scrollbar", id: "scrollVer", group: "vertical" }
                ]
            },
            { resizer: true, width: 1, next: "resources" },
            {
                height: 35,
                cols: [
                    { html: "", group: "grids" },
                    { resizer: true, width: 1 },
                    { html: "" }
                ]
            },
            //{
            //    gravity: 1,
            //    id: "resources",
            //    config: resourceConfig,
            //    templates: resourceTemplates,
            //    cols: [
            //        { view: "resourceGrid", group: "grids", scrollY: "resourceVScroll" },
            //        { resizer: true, width: 1 },
            //        { view: "resourceTimeline", scrollX: "scrollHor", scrollY: "resourceVScroll" },
            //        { view: "scrollbar", id: "resourceVScroll", group: "vertical" }
            //    ]
            //},
            { view: "scrollbar", id: "scrollHor" }
        ]
    };

    gantt.$resourcesStore = gantt.createDatastore({
        name: gantt.config.resource_store,
        type: "treeDatastore",
        initItem: function (item) {
            item.parent = item.parent || gantt.config.root_id;
            item[gantt.config.resource_property] = item.parent;
            item.open = true;
            return item;
        }
    });


    gantt.$resourcesStore.attachEvent("onFilterItem", function (id, item) {
        return gantt.getResourceAssignments(id).length > 0;
    });

    gantt.init("gantt_here");
    gantt.$resourcesStore.attachEvent("onParse", function () {
        var people = [];
        gantt.$resourcesStore.eachItem(function (res) {
            if (!gantt.$resourcesStore.hasChild(res.id)) {
                var copy = gantt.copy(res);
                copy.key = res.id;
                copy.label = res.text;
                people.push(copy);
            }
        });
        gantt.updateCollection("people", people);
    });
    gantt.attachEvent("onParse", function () {
        gantt.render();
    });
    //gantt.$resourcesStore.parse([
    //    { id: 1, text: "Anna, Architect", unit: "hours/day", default_value: 8, type: "work" },
    //    { id: 2, text: "Finn, Construction worker", unit: "hours/day", default_value: 8, type: "work" },
    //    { id: 3, text: "Jake, Construction worker", unit: "hours/day", default_value: 8, type: "work" },
    //    { id: 4, text: "Floe, Plasterer", unit: "hours/day", default_value: 8, type: "work" },
    //    { id: 5, text: "Tom, Plumber", unit: "hours/day", default_value: 8, type: "work" },
    //    { id: 6, text: "Mike, Electrician", unit: "hours/day", default_value: 8, type: "work" },
    //    { id: 7, text: "Joe, Handyman", unit: "hours/day", default_value: 8, type: "work" },
    //    { id: 8, text: "Concrete", unit: "m3", default_value: 1 },
    //    { id: 9, text: "Blocks", unit: "m3", default_value: 1 },
    //    { id: 10, text: "Air conditioners", unit: "units", default_value: 1 },
    //    { id: 11, text: "Radiators", unit: "units", default_value: 1 },
    //    { id: 12, text: "Pipes", unit: "meters", default_value: 5 },
    //    { id: 13, text: "Wires", unit: "meters", default_value: 10 },
    //    { id: 14, text: "Paint", unit: "cans%", default_value: 1 }

    //]);
    // gantt.parse(taskData);

    $.ajax({
        url: "/Home/LoadUserProjectAjax/",
        type: "POST",
        dataType: "json",

        success: function (response) {
            if (response.Data != '') {
                gantt.$resourcesStore.parse(response.Data);
            };
        }
    });
    gantt.load("/Home/Data", "json");

    //// enable dataProcessor
    //var dp = new dataProcessor("/Home/Save/");
    //dp.init(gantt);

    // enable dataProcessor
    var dp = new gantt.dataProcessor("/Home/Save/");
    dp.init(gantt);
})();