<%@ Page Language="C#" Culture="en-US" AutoEventWireup="true" CodeBehind="KiepVisitorRegistration.aspx.cs" Inherits="KiepVisitorRegistration._Default" %>
<%
    const int DAYS_VISIBLE_NORMAL = 14;
    const int DAYS_VISIBLE_AUTHENTICATED = 365;
    const int ADD_TIMESLOT_NUMBER_OF_MONTHS = 2;
    const int ADD_TIMESLOT_SELECTED_HOUR = 19;
    const double ADD_TIMESLOT_INTERVAL_HOURS = 0.5;
    const string ADD_TIMESLOT_SELECTED_DURIATION = "60";


    // TODO Hard-code a username (no password) with rights to add/remove timeslots and remove visitors
    const string ADMIN_USERNAME = "my_admin_username";


    bool authenticated = false;
    int daysVisible = DAYS_VISIBLE_NORMAL;
    string message = "";
    if (Request.QueryString["user"] != null && Request.QueryString["user"] == ADMIN_USERNAME)
    {
        authenticated = true;
        daysVisible = DAYS_VISIBLE_AUTHENTICATED;

        // Add timeslot
        if (Request.QueryString["add_timeslot"] != null && Request.QueryString["add_timeslot"] == "true")
        {
            try
            {
                Appointment appointment = new Appointment();
                appointment.from = DateTime.Parse(Request.QueryString["date"] + " " + Request.QueryString["start_time"]);

                int duration = int.Parse(Request.QueryString["duration"]);
                appointment.to = DateTime.Parse(Request.QueryString["date"] + " " + Request.QueryString["start_time"]).AddMinutes(duration);
                AddTimeslot(appointment);
                message = "<b>Timeslot is added.</b><br><br>";
            }
            catch (Exception)
            {
                message = "<b>Timeslot is NOT added.</b><br><br>";
            }
        }

        // Add timeslots whole month
        if (Request.QueryString["add_timeslot_month"] != null && Request.QueryString["add_timeslot_month"] == "true")
        {
            try
            {
                AddTimeslotWholeMonth(int.Parse(Request.QueryString["month"]));
                message = "<b>Timeslots are added.</b><br><br>";
            }
            catch (Exception)
            {
                message = "<b>Timeslots are NOT added.</b><br><br>";
            }
        }

        // Remove timeslot
        if (Request.QueryString["remove_timeslot"] != null)
        {
            try
            {
                Appointment appointment = new Appointment();
                appointment.id = int.Parse(Request.QueryString["remove_timeslot"]);
                RemoveTimeslot(appointment);
                message = "<b>Timeslot is removed.</b><br><br>";
            }
            catch (Exception)
            {
                message = "<b>Timeslot is NOT removed.</b><br><br>";
            }
        }

        // Un-schedule appointment
        if (Request.QueryString["remove_appointment"] != null)
        {
            try
            {
                Appointment appointment = new Appointment();
                appointment.id = int.Parse(Request.QueryString["remove_appointment"]);
                UnscheduleAppointment(appointment);
                message = "<b>Appointment is removed.</b><br><br>";
            }
            catch (Exception)
            {
                message = "<b>Appointment is NOT removed.</b><br><br>";
            }
        }
    }

%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>KiepVisitorRegistration</title>
    <link rel="stylesheet" type="text/css" href="KiepVisitorRegistration.css" />
</head>
<body style="background:none">
    
    <% if (authenticated) Response.Write(message); %>
    <br />
    
        <table style="width:100%;">
            <tr>
                <td>
                    <h3>Overview</h3></td>
                <td>
                    &nbsp;</td>
            </tr>
                <%
                    try
                    {

                        if (Request.QueryString["timeslot"] != null && Request.QueryString["name"] != null)
                        {
                            try
                            {
                                if (Request.QueryString["name"] == string.Empty)
                                {
                                    throw new Exception("No name entered.");
                                }
                                Appointment appointment = new Appointment();
                                appointment.id = int.Parse(Request.QueryString["timeslot"]);
                                appointment.name = Request.QueryString["name"];
                                appointment.phone = Request.QueryString["phone"];
                                ScheduleAppointment(appointment);
                                Response.Write("<b>Visit is registered.</b><br><br>");
                            }
                            catch (Exception)
                            {
                                Response.Write("<b>Visit is not registered. Enter a valid time, name and phone number.</b><br><br>");
                            }
                        }

                        foreach (Appointment appointment in GetAppointmentOverview(daysVisible))
                        {
                            string nameOrAvailable = appointment.name;
                            if (nameOrAvailable == string.Empty)
                            {
                                nameOrAvailable = "<b>available</b>";
                            }
                            Response.Write("<tr><td>" + appointment.from.ToString("dddd d MMMM") + " from " + appointment.from.ToShortTimeString() + " to " + appointment.to.ToShortTimeString() + "</td><td>" + nameOrAvailable + "</td>");

                            if (authenticated)
                            {
                                Response.Write("<td>" + appointment.phone + "</td>");
                                Response.Write("<td>&nbsp;");
                                if (appointment.name != string.Empty)
                                {
                                    Response.Write("<a href=\"?user=" + Request.QueryString["user"] + "&remove_appointment=" + appointment.id + "\">clear</a>");
                                }
                                Response.Write("</td>");
                                Response.Write("<td><a href=\"?user=" + Request.QueryString["user"] + "&remove_timeslot=" + appointment.id + "\">remove timeslot</a></td>");
                            }
                            Response.Write("</tr>");
                        }
                    }
                    catch (Exception) { }
            %>
         </table>
        <br />
    <form action="">
    <input type="hidden" name="user" value="<%=Request.QueryString["user"]%>" />
        <table style="width: 41%;">
            <tr>
                <td>
                    <h3>Registration</h3></td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    Time:</td>
                <td>
                    <select name="timeslot">
                        <%
                            try
                            {
                                foreach (Appointment appointment in GetAvailableTimeslots(daysVisible))
                                {
                                    Response.Write("<option value=\"" + appointment.id + "\">" + appointment.from.ToString("dddd d MMMM") + " from " + appointment.from.ToShortTimeString() + " to " + appointment.to.ToShortTimeString() + "</option>");
                                }
                            }
                            catch (Exception) { }
                        %>
                    </select>
                </td>
            </tr>
            <tr>
                <td>
                    Name:<br />
                    <font size="-2">Max. 1 person</font></td>
                <td>
                    <input name="name" type="text" size="40" /></td>
            </tr>
            <tr>
                <td>
                    Phone number:
                </td>
                <td>
                    <input name="phone" type="text" size="40" /></td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td>
                    <input type="submit" /></td>
            </tr>
        </table>
    </form>
        <p>&nbsp;</p>

<% if (authenticated)
   { %>
    <form action="">
    <input type="hidden" name="add_timeslot" value="true" />
    <input type="hidden" name="user" value="<%=Request.QueryString["user"]%>" />
       <table style="width: 41%;">
            <tr>
                <td>
                    <h3>Add timeslot</h3></td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    Date:</td>
                <td>
                    <select name="date">
                        <%
                            try
                            {
                                System.Collections.Generic.List<DateTime> datumLijst = GenerateDateList(DateTime.Now, DateTime.Now.AddMonths(ADD_TIMESLOT_NUMBER_OF_MONTHS));

                                foreach (DateTime datum in datumLijst)
                                {
                                    Response.Write("<option value=\"" + datum.ToShortDateString() + "\">" + datum.ToString("dddd d MMMM") + "</option>");
                                }
                            }
                            catch (Exception) { }
                        %>
                    </select>
                </td>
            </tr>
            <tr>
                <td>
                    Start time:</td>
                <td>
                    <select name="start_time">
                        <%
                            try
                            {
                                System.Collections.Generic.List<DateTime> timeList = GenerateTimeList(ADD_TIMESLOT_INTERVAL_HOURS);

                                DateTime selectedTime;
                                if (Request.QueryString["start_time"] != null && Request.QueryString["start_time"] != "")
                                {
                                    selectedTime = DateTime.Parse("01/01/0001 " + Request.QueryString["start_time"]);
                                }
                                else
                                {
                                    selectedTime = new DateTime(1, 1, 1, ADD_TIMESLOT_SELECTED_HOUR, 0, 0);
                                }

                                foreach (DateTime time in timeList)
                                {
                                    string selected = "";
                                    if (time.Equals(selectedTime))
                                    {
                                        selected = " selected";
                                    }

                                    Response.Write("<option" + selected + " value=\"" + time.ToShortTimeString() + "\">" + time.ToShortTimeString() + "</option>");
                                }
                            }
                            catch (Exception) { }
                        %>
                    </select>
                </td>
            </tr>
            <tr>
                <td>
                    Duration (minutes):</td>
                <td>
                    <select name="duration">
                        <% 
                            string selectedDuration = Request.QueryString["duration"];
                            if (selectedDuration == null || selectedDuration == "")
                            {
                                selectedDuration = ADD_TIMESLOT_SELECTED_DURIATION;
                            }   
                        %>
                        <option value="10" <% if(selectedDuration == "10") Response.Write("selected"); %>>10</option>
                        <option value="15" <% if(selectedDuration == "15") Response.Write("selected"); %>>15</option>
                        <option value="20" <% if(selectedDuration == "20") Response.Write("selected"); %>>20</option>
                        <option value="30" <% if(selectedDuration == "30") Response.Write("selected"); %>>30</option>
                        <option value="45" <% if(selectedDuration == "45") Response.Write("selected"); %>>45</option>
                        <option value="60" <% if(selectedDuration == "60") Response.Write("selected"); %>>60</option>
                        <option value="120" <% if(selectedDuration == "120") Response.Write("selected"); %>>120</option>
                    </select>
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td>
                    <input type="submit" /></td>
            </tr>
        </table>
    </form>
    <p>&nbsp;</p>
    
    
    <form action="">
    <input type="hidden" name="add_timeslot_month" value="true" />
    <input type="hidden" name="user" value="<%=Request.QueryString["user"]%>" />
       <table style="width: 41%;">
            <tr>
                <td>
                    <h3>Add timeslots for whole month</h3></td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td colspan="2">
                    Mon-Fri 19:30-20:30<br />
                    Saturday 14:30-15:30, 19:30-20:30<br />
                    Sunday 11:30-12:30, 14:30-15:30, 19:30-20:30
                </td>
            </tr>
            <tr>
                <td>
                    Month:</td>
                <td>
                    <select name="month">
                        <%
                            try
                            {
                                for (int i = 0; i < 12; i++)
                                {
                                    DateTime month = new DateTime(1, ((i + DateTime.Now.Month) % 12) + 1, 1);
                                    Response.Write("<option value=\"" + month.ToString("MM") + "\">" + month.ToString("MMMM") + "</option>");
                                }
                            }
                            catch (Exception) { }
                        %>
                    </select>
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td>
                    <input type="submit" /></td>
            </tr>
        </table>
    </form>
    <%} %>
</body>
</html>
