using System;
using System.Collections.Generic;
using System.Text;

namespace EmailNotifications
{
   
        public static class TableStructure
        {
            public class Table : HtmlBase, IDisposable
            {
                public Table(StringBuilder sb, string classAttributes = "", string id = "", string align ="") : base(sb)
                {
                    Append("<table"+ $" align=\"{align}\"");
                    AddOptionalAttributes(classAttributes, id);
                }

                public void StartHead(string classAttributes = "", string id = "")
                {
                    Append("<thead");
                    AddOptionalAttributes(classAttributes, id);
                }

                public void EndHead()
                {
                    Append("</thead>");
                }

                public void StartFoot(string classAttributes = "", string id = "")
                {
                    Append("<tfoot");
                    AddOptionalAttributes(classAttributes, id);
                }

                public void EndFoot()
                {
                    Append("</tfoot>");
                }

                public void StartBody(string classAttributes = "", string id = "")
                {
                    Append("<tbody");
                    AddOptionalAttributes(classAttributes, id);
                }

                public void EndBody()
                {
                    Append("</tbody>");
                }

                public void Dispose()
                {
                    Append("</table>");
                }

                public Row AddRow(string classAttributes = "", string id = "")
                {
                    return new Row(GetBuilder(), classAttributes, id);
                }
            }

            public class Row : HtmlBase, IDisposable
            {
                public Row(StringBuilder sb, string classAttributes = "", string id = "") : base(sb)
                {
                    Append("<tr");
                    AddOptionalAttributes(classAttributes, id);
                }
                public void Dispose()
                {
                    Append("</tr>");
                }
                public void AddCell(string innerText, string classAttributes = "", string id = "", string colSpan = "",string align ="")
                {
                    Append("<td");
                    AddOptionalAttributes(classAttributes, id, colSpan,align);
                    Append(innerText);
                    Append("</td>");
                }
            }

            public abstract class HtmlBase
            {
                private StringBuilder _sb;

                protected HtmlBase(StringBuilder sb)
                {
                    _sb = sb;
                }

                public StringBuilder GetBuilder()
                {
                    return _sb;
                }

                protected void Append(string toAppend)
                {
                    _sb.Append(toAppend);
                }

                protected void AddOptionalAttributes(string className = "", string id = "", string colSpan = "",string align ="")
                {

                    if (!string.IsNullOrEmpty(id))
                    {
                        _sb.Append($" id=\"{id}\"");
                    }
                    if (!string.IsNullOrEmpty(className))
                    {
                        _sb.Append($" class=\"{className}\"");
                    }
                    if (!string.IsNullOrEmpty(colSpan))
                    {
                        _sb.Append($" colspan=\"{colSpan}\"");
                    }
                    if (!string.IsNullOrEmpty(colSpan))
                    {
                        _sb.Append($" align=\"{align}\"");
                    }
                _sb.Append(">");
                }
            }
        }
    }

