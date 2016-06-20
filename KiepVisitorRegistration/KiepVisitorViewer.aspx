<%@ Page Language="C#" Culture="en-US" AutoEventWireup="true" CodeBehind="KiepVisitorRegistration.aspx.cs" Inherits="KiepVisitorRegistration._Default" %>
<%
    const int APPOINTMENTS_PER_PAGE = 12;
    const int PAGES_HISTORY = 1;
    const int PAGES_FUTURE = 2;

    StringBuilder appointments = new StringBuilder();
    int appointmentCount = 1;
    DateTime previous = new DateTime();
    bool pagebreakSkipped = false;
    try
    {

        System.Collections.Generic.List<Appointment> appointmentList = new System.Collections.Generic.List<Appointment>();
        appointmentList.AddRange(GetAppointmentsHistory(APPOINTMENTS_PER_PAGE * PAGES_HISTORY));
        appointmentList.AddRange(GetAppointments(APPOINTMENTS_PER_PAGE * PAGES_FUTURE));

        foreach (Appointment appointment in appointmentList)
        {
            string nameOrAvailable = appointment.name;
            if (nameOrAvailable == string.Empty)
            {
                nameOrAvailable = "<available>";
            }


            if (previous != appointment.from)
            {
                if (pagebreakSkipped || (appointmentCount > 1 && appointmentCount % APPOINTMENTS_PER_PAGE == 1))
                {
                    // Break pages
                    appointments.Append("<page-break>");
                    appointments.Append("\r\n");
                    pagebreakSkipped = false;
                }
                else if (appointmentCount > 1)
                {
                    // Text blocks
                    appointments.Append("<new-block>");
                    appointments.Append("\r\n");
                }

                // Write appointment including start day+time
                int diff = appointment.from.DayOfYear - DateTime.Now.DayOfYear;
                if (appointment.from.Year > DateTime.Now.Year)
                {
                    diff += 365;
                }


                string from;
                if (diff == 2)
                {
                    from = "day after tomorrow" + " " + appointment.from.ToShortTimeString();
                }
                else if (diff == 1)
                {
                    from = "tomorrow" + " " + appointment.from.ToShortTimeString();
                }
                else if (diff == 0)
                {
                    from = "today" + " " + appointment.from.ToShortTimeString();
                }
                else if (diff == -1)
                {
                    from = "yesterday" + " " + appointment.from.ToShortTimeString();
                }
                else if (diff == -2)
                {
                    from = "day before yesterday" + " " + appointment.from.ToShortTimeString();
                }
                else if (diff < 0 && diff >= -7)
                {
                    from = "this week";
                }
                else if (diff < -7 && diff >= -14)
                {
                    from = "last week";
                }
                else if (diff < -14)
                {
                    from = "before\t";
                }
                else
                {
                    from = appointment.from.ToString("dddd") + " " + appointment.from.Day + "-" + appointment.from.Month + " " + appointment.from.ToShortTimeString();;
                }
                appointments.Append(from);

                if (from.Length < 15)
                {
                    appointments.Append("\t");
                }
                else
                {
                    appointments.Append(" ");
                }
                appointments.Append(nameOrAvailable);
                appointments.Append("\r\n");
            }
            else
            {
                // Never break pages when displaying same appointment-date/time
                if (appointmentCount > 1 && appointmentCount % APPOINTMENTS_PER_PAGE == 1)
                {
                    pagebreakSkipped = true;
                }

                // Write appointment without start day+time
                appointments.Append("\t\t");
                appointments.Append(nameOrAvailable);
                appointments.Append("\r\n");
            }

            previous = appointment.from;
            appointmentCount++;
        }

        appointments = appointments.Remove(appointments.Length - 2, 2);
        appointments = appointments.Replace("  ", " ");
        Response.Write(appointments.ToString());
    }
    catch (Exception) { }
%>