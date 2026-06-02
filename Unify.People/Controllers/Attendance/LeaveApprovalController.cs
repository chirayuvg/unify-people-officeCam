using AppStudio.Framework.MVC.Controllers;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using Unify.People.Models;
using Unify.People.Widgets;

namespace Unify.People.Controllers.Attendance
{
    public class LeaveApprovalController : PageController
    {
        private CardView<EmployeeFilterData> cardView;
        private GridViewLeaveApproval<LeaveApproval> gridViewLeaveApproval;

        public override void Init()
        {
            this.Page.Title = "Leave Approval";
            this.Page.ShowScrollBar = true;

            cardView = this.AddCardView<EmployeeFilterData>().SetControlStyle(CardViewStyle.Filter);
            cardView.ControlStyle = CardViewStyle.Filter;
            cardView.SubmitOnDataChange = true;
            cardView.LabelPosition = LabelPosition.Left;
            cardView.OnSubmit += CardView_OnSubmit;

            if (cardView.Data.Period == Period.CustomDateRange)
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = false;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = false;
            }
            else
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = true;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = true;
            }

            gridViewLeaveApproval = this.AddControl<GridViewLeaveApproval<LeaveApproval>>();
            gridViewLeaveApproval
                .ConfigueUI(grid => grid
                    .ColumnAutoWidth(false)
                    .FilterRow(f => f.Visible(false))
                    .HeaderFilter(headerFilter => headerFilter.Visible(false))
                    .RowAlternationEnabled(true)
                    .Scrolling(scrolling => scrolling.ColumnRenderingMode(GridColumnRenderingMode.Virtual))
                    .SearchPanel(s => s.Visible(false))
                    .ShowBorders(true)
                    .ShowColumnHeaders(true)
                    .ShowRowLines(false).ShowColumnLines(false)
                    .WordWrapEnabled(true)
                    .Paging(paging => paging.Enabled(true).PageSize(5)) //  Enable paging with 5 rows per page
                    .Pager(pager =>
                    {
                        pager
                            .Visible(true)
                            .DisplayMode(GridPagerDisplayMode.Full)     //  Show full pagination with navigation
                            .ShowPageSizeSelector(false)                //  Remove option to change page size
                            .AllowedPageSizes(new JS("[5]"))            //  Only allow 5 rows per page
                            .ShowInfo(true)                             //  Show page numbers (e.g., "Page 1 of 8")
                            .ShowNavigationButtons(true);               //  Enable page navigation
                    })
                );
            //gridViewMyLeaves.Css.SetHeight("calc(100vh - 290px)");
            gridViewLeaveApproval.Fields(field =>
            {
                field.ClearFields();

                field.AddField(f => f.Employee);
                field.AddField(f => f.StartDate);
                field.AddField(f => f.EndDate);
                field.AddField(f => f.LeavePolicy);
                field.AddField(f => f.LeaveRemarks);
                field.AddField(f => f.AppliedOn);
                field.AddField(f => f.ApprovalStatus);
                field.AddField(f => f.ApprovalRemarks);
            });

            var btnApprove = gridViewLeaveApproval.AddButton("Approve");
            btnApprove.OnClick += BtnApprove_OnClick;

            var btnReject = gridViewLeaveApproval.AddButton("Reject");
            btnReject.OnClick += BtnReject_OnClick;

            UiAction.AddJsCode(@"
                (function setUniformFontStyles() {
                    const selectors = [
                        '.dx-toolbar-label .dx-toolbar-item-content > div',
                        '.dx-datagrid .dx-header-row > td',
                        '.dx-datagrid .dx-row',
                        '.dx-toolbar .dx-item-content',
                        '.dx-datagrid .dx-datagrid-content .dx-row > td',
                        '.dx-card .dx-item-content',
                        '.dx-datagrid .dx-pager',
                        'h1, h2, h3, h4',
                        '.card-value',       
                        '.card-text'
                    ];

                    // Apply styles to regular HTML elements
                    selectors.forEach(selector => {
                        document.querySelectorAll(selector).forEach(el => {
                            el.style.fontSize = '15px';
                            el.style.fontFamily = 'Segoe UI, sans-serif';
                            el.style.lineHeight = '1.4';
                            el.style.marginLeft = '0';
                            el.style.marginRight = '0';
                        });
                    });

                    // Handle DevExpress Chart SVG Titles
                    document.querySelectorAll('.dxc-title text').forEach(el => {
                            el.style.fontSize = '15px';
                            el.style.fontFamily = 'Segoe UI, sans-serif';
                            el.style.lineHeight = '1.4';
                            el.style.marginLeft = '0';
                            el.style.marginRight = '0';
                    });

                document.querySelectorAll('.dx-toolbar-item-content div').forEach(el => {
                            el.style.fontSize = '15px';
                            el.style.fontFamily = 'Segoe UI, sans-serif';
                            el.style.lineHeight = '1.4';
                            el.style.marginLeft = '0';
                            el.style.marginRight = '0';
                });

                })();
            ");
        }

        private async Task CardView_OnSubmit(EmployeeFilterData model, IFormFileCollection files)
        {
            model.GetDates();
            model.GetEmployeeName();

            if (cardView.Data.Period == Period.CustomDateRange)
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = false;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = false;
            }
            else
            {
                cardView.GetFieldControl(m => m.FromDate).IsReadOnly = true;
                cardView.GetFieldControl(m => m.ToDate).IsReadOnly = true;
            }
        }

        private void BtnApprove_OnClick(ButtonItem obj)
        {
            UiAction.ShowPopup("Approve Leave", "ApproveLeave", "Index", new { LeaveId = obj.RecordId, ApprovalStatus = "Approved" });
        }

        private async void BtnReject_OnClick(ButtonItem obj)
        {
            UiAction.ShowPopup("Reject Leave", "ApproveLeave", "Index", new { LeaveId = obj.RecordId, ApprovalStatus = "Rejected" });
        }
    }
}
