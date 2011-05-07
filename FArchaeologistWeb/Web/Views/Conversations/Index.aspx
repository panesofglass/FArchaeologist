<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<html>
  <head>
    <title>Conversations | Alt.Net Archaeologist</title>

    <script src="/scripts/protovis.js" type="text/javascript"></script>
    <script src="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.6.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $.ajax({
                url: '/conversation_edges',
                success: function (data) {
                    var nodes = data["nodes"];
                    var links = data["links"];

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
    <a href="/">back to archaeologist</a>
    <p>
      Darker colors mean more people are mentioning that person.<br />
      Lighter colors means few, if anyone, is tweeting about that person.
    </p>
  </body>
</html>
