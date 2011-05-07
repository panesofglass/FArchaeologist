﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<!DOCTYPE html />
<html>
  <head>
    <title>FArchaeologist</title>

    <script src="/scripts/protovis.js" language="javascript"></script>
    <script src="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.6.min.js" language="javascript"></script>
    <script language="Javascript">
        function mentionGroup(count) {
            return Math.floor(count / 10) + 1;
        }

        $(document).ready(function () {
            $.ajax({
                url: '/conversation_edges',
                success: function (data) {
                    var mentioned_counts = {};

                    var node_names = {};
                    for (var index in data) {
                        var item = data[index];
                        var sender = item["sender"];

                        if (node_names[sender] == null) {
                            node_names[sender] = {};
                        }

                        var mentions = item["mentions"];
                        for (var mention_index in mentions) {
                            var mention = mentions[mention_index];

                            if (node_names[mention] == null) {
                                node_names[mention] = {};
                            }

                            if (mentioned_counts[mention] == null) {
                                mentioned_counts[mention] = 0;
                            }

                            mentioned_counts[mention] += 1;

                            node_names[sender][mention] = true;
                        }
                    }

                    var nodes = [];
                    var node_indices = {};
                    var node_start_index = 0;
                    for (var node_index in node_names) {
                        var group = mentioned_counts[node_index];
                        nodes.push({ "nodeName": node_index, "group": mentionGroup(group) });
                        node_indices[node_index] = node_start_index;
                        node_start_index += 1;
                    }

                    var links = [];
                    for (var node_index in node_names) {
                        var sender_name = node_index;
                        var those_mentioned_by_sender = node_names[node_index];

                        for (var user_mentioned in those_mentioned_by_sender) {
                            var link = {
                                "source": node_indices[sender_name],
                                "target": node_indices[user_mentioned],
                                "value": 1
                            };
                            links.push(link);
                        }
                    }

                    var w = document.body.clientWidth;
                    var h = document.body.clientHeight;
                    var colors = pv.Colors.category19();

                    var vis = new pv.Panel()
                .width(w)
                .height(h)
                .fillStyle("white")
                .event("mousedown", pv.Behavior.pan())
                .event("mousewheel", pv.Behavior.zoom());

                    var force = vis.add(pv.Layout.Force)
                .nodes(nodes)
                .links(links);

                    force.link.add(pv.Line);

                    force.node.add(pv.Dot)
                .size(function (d) {
                    return (d.linkDegree + 4) * Math.pow(this.scale, -1.5);
                })
                .fillStyle(function (d) { return d.fix ? "brown" : colors(d.group); })
                .strokeStyle(function () { return this.fillStyle().darker(); })
                .lineWidth(1)
                .title(function (d) { return d.nodeName; })
                .event("mousedown", pv.Behavior.drag())
                .event("drag", force);

                    vis.render();
                },
                dataType: 'json'
            });
        });
    </script>
  </head>
  <body>
    <h2>Who's Talking to Who?</h2>
    <a href="http://archaeologist.heroku.com/">back to archaeologist</a>
    <p>
      Darker colors mean more people are mentioning that person.<br />
      Lighter colors means few, if anyone, is tweeting about that person.
    </p>
  </body>
</html>
