using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;

namespace KiepVisitorRegistration
{

    /**
     * Download MySQL ODBC connector from https://dev.mysql.com/downloads/connector/odbc/
     * If it is not working, try to install the 32 bits version even if you have a 64 bits machine.
     * 
     * MySQL table definition:
     * 
     * CREATE TABLE IF NOT EXISTS `KiepVisitorRegistration` (
     *      `id` int(11) NOT NULL AUTO_INCREMENT,
     *      `start_time` datetime NOT NULL,
     *      `end_time` datetime NOT NULL,
     *      `name` varchar(255) NOT NULL DEFAULT '',
     *      `phone` varchar(255) NOT NULL DEFAULT '',
     *      PRIMARY KEY (`id`)
     * ) ENGINE=MyISAM DEFAULT CHARSET=latin1;
     */

    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        // TODO Enter your MySQL connection details
        private const string connectionString = "DRIVER={MySQL ODBC 5.3 Unicode Driver};SERVER=127.0.0.1;DATABASE=joozt;UID=test;PASSWORD=test;OPTION=3";


        public struct Appointment
        {
            public int id;
            public DateTime from;
            public DateTime to;
            public string name;
            public string phone;
        }

        public static List<Appointment> GetAppointmentOverview(int numberOfDays)
        {
            List<Appointment> result = new List<Appointment>();
            using (IDbConnection connection = new OdbcConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM KiepVisitorRegistration WHERE (start_time BETWEEN NOW() AND date_add(NOW(), INTERVAL ? DAY)) ORDER BY start_time, name";
                    command.Parameters.Add(new OdbcParameter("@interval", numberOfDays));
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Appointment appointment = new Appointment();
                            appointment.id = (int)reader["id"];
                            appointment.from = (DateTime)reader["start_time"];
                            appointment.to = (DateTime)reader["end_time"];
                            appointment.name = (string)reader["name"];
                            appointment.phone = (string)reader["phone"];
                            result.Add(appointment);
                        }
                    }
                }
                return result;
            }
        }

        public static List<Appointment> GetAppointments(int numberOfAppointments)
        {
            List<Appointment> result = new List<Appointment>();
            using (IDbConnection connection = new OdbcConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM KiepVisitorRegistration WHERE (start_time > NOW()) ORDER BY start_time, name LIMIT 0, ?";
                    command.Parameters.Add(new OdbcParameter("@numberOfAppointments", numberOfAppointments));
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Appointment appointment = new Appointment();
                            appointment.id = (int)reader["id"];
                            appointment.from = (DateTime)reader["start_time"];
                            appointment.to = (DateTime)reader["end_time"];
                            appointment.name = (string)reader["name"];
                            appointment.phone = (string)reader["phone"];
                            result.Add(appointment);
                        }
                    }
                }
                return result;
            }
        }

        public static List<Appointment> GetAppointmentsHistory(int numberOfAppointments)
        {
            List<Appointment> result = new List<Appointment>();
            using (IDbConnection connection = new OdbcConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM KiepVisitorRegistration WHERE (name != \"\") AND (start_time < NOW()) ORDER BY start_time DESC, name LIMIT 0, ?";
                    command.Parameters.Add(new OdbcParameter("@numberOfAppointments", numberOfAppointments));
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Appointment appointment = new Appointment();
                            appointment.id = (int)reader["id"];
                            appointment.from = (DateTime)reader["start_time"];
                            appointment.to = (DateTime)reader["end_time"];
                            appointment.name = (string)reader["name"];
                            appointment.phone = (string)reader["phone"];
                            result.Add(appointment);
                        }
                    }
                }
                return result;
            }
        }

        public static List<Appointment> GetAvailableTimeslots(int numberOfDays)
        {
            List<Appointment> result = new List<Appointment>();
            using (IDbConnection connection = new OdbcConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM KiepVisitorRegistration WHERE name = \"\" AND (start_time BETWEEN NOW() AND date_add(NOW(), INTERVAL ? DAY)) ORDER BY start_time, name";
                    command.Parameters.Add(new OdbcParameter("@interval", numberOfDays));
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Appointment appointment = new Appointment();
                            appointment.id = (int)reader["id"];
                            appointment.from = (DateTime)reader["start_time"];
                            appointment.to = (DateTime)reader["end_time"];
                            result.Add(appointment);
                        }
                    }
                }
                return result;
            }
        }

        public static void ScheduleAppointment(Appointment appointment)
        {
            using (IDbConnection connection = new OdbcConnection(connectionString))
            {
                connection.Open();

                // Check if not modifying existing appointment
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM KiepVisitorRegistration WHERE id = ? AND name = \"\"";
                    command.Parameters.Add(new OdbcParameter("@id", appointment.id));
                    Int64 numberOfRecords = (Int64)command.ExecuteScalar();
                    if (numberOfRecords != 1)
                    {
                        throw new Exception("No empty timeslot found with ID " + appointment.id);
                    }
                }

                // Update appointment -> add name and phone number
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE KiepVisitorRegistration SET name = ?, phone = ? WHERE id = ?";
                    command.Parameters.Add(new OdbcParameter("@name", appointment.name));
                    command.Parameters.Add(new OdbcParameter("@phone", appointment.phone));
                    command.Parameters.Add(new OdbcParameter("@id", appointment.id));
                    command.ExecuteNonQuery();
                }

            }
        }

        public static void UnscheduleAppointment(Appointment appointment)
        {
            using (IDbConnection connection = new OdbcConnection(connectionString))
            {
                connection.Open();

                // Update appointment -> remove name and phone number
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE KiepVisitorRegistration SET name = '', phone = '' WHERE id = ? LIMIT 1";
                    command.Parameters.Add(new OdbcParameter("@id", appointment.id));
                    command.ExecuteNonQuery();
                }

            }
        }

        public static void RemoveTimeslot(Appointment appointment)
        {
            using (IDbConnection connection = new OdbcConnection(connectionString))
            {
                connection.Open();

                // Delete appointment
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM KiepVisitorRegistration WHERE id = ? LIMIT 1";
                    command.Parameters.Add(new OdbcParameter("@id", appointment.id));
                    command.ExecuteNonQuery();
                }

            }
        }

        public static void AddTimeslot(Appointment appointment)
        {
            using (IDbConnection connection = new OdbcConnection(connectionString))
            {
                connection.Open();

                // Insert new
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO KiepVisitorRegistration (start_time, end_time) VALUES (?, ?)";
                    command.Parameters.Add(new OdbcParameter("@start_time", appointment.from));
                    command.Parameters.Add(new OdbcParameter("@end_time", appointment.to));
                    command.ExecuteNonQuery();
                }

            }
        }

        public static void AddTimeslotWholeMonth(int month)
        {
            DateTime appointmentTime = new DateTime(1, 1, 1, 19, 30, 0);
            DateTime appointmentTimeSaturdayAfternoon = new DateTime(1, 1, 1, 14, 30, 0);
            DateTime appointmentTimeSundayMorning = new DateTime(1, 1, 1, 11, 30, 0);
            DateTime appointmentTimeSundayAfternoon = new DateTime(1, 1, 1, 14, 30, 0);
            int appointmentDuriation = 60;


            int year = DateTime.Now.Year;
            if (month < DateTime.Now.Month)
            {
                year++;
            }

            List<DateTime> daysInMonth = GenerateDateList(new DateTime(year, month, 1), (new DateTime(year, month, 1)).AddMonths(1));
            foreach (DateTime day in daysInMonth)
            {
                Appointment appointment = new Appointment();
                appointment.from = new DateTime(day.Year, day.Month, day.Day, appointmentTime.Hour, appointmentTime.Minute, 0);
                appointment.to = appointment.from.AddMinutes(appointmentDuriation);
                AddTimeslot(appointment);
                AddTimeslot(appointment);

                if (appointment.from.DayOfWeek == DayOfWeek.Saturday)
                {
                    Appointment appointmentSaturdayAfternoon = new Appointment();
                    appointmentSaturdayAfternoon.from = new DateTime(day.Year, day.Month, day.Day, appointmentTimeSaturdayAfternoon.Hour, appointmentTimeSaturdayAfternoon.Minute, 0);
                    appointmentSaturdayAfternoon.to = appointmentSaturdayAfternoon.from.AddMinutes(appointmentDuriation);
                    AddTimeslot(appointmentSaturdayAfternoon);
                    AddTimeslot(appointmentSaturdayAfternoon);
                }

                if (appointment.from.DayOfWeek == DayOfWeek.Sunday)
                {
                    Appointment appointmentSundayMorning = new Appointment();
                    appointmentSundayMorning.from = new DateTime(day.Year, day.Month, day.Day, appointmentTimeSundayMorning.Hour, appointmentTimeSundayMorning.Minute, 0);
                    appointmentSundayMorning.to = appointmentSundayMorning.from.AddMinutes(appointmentDuriation);
                    AddTimeslot(appointmentSundayMorning);
                    AddTimeslot(appointmentSundayMorning);

                    Appointment appointmentSundayAfternoon = new Appointment();
                    appointmentSundayAfternoon.from = new DateTime(day.Year, day.Month, day.Day, appointmentTimeSundayAfternoon.Hour, appointmentTimeSundayAfternoon.Minute, 0);
                    appointmentSundayAfternoon.to = appointmentSundayAfternoon.from.AddMinutes(appointmentDuriation);
                    AddTimeslot(appointmentSundayAfternoon);
                    AddTimeslot(appointmentSundayAfternoon);
                }
            }
        }

        public static List<DateTime> GenerateDateList(DateTime from, DateTime to)
        {
            List<DateTime> result = new List<DateTime>();
            while (from < to)
            {
                result.Add(from);
                from = from.AddDays(1);
            }
            return result;
        }

        public static List<DateTime> GenerateTimeList(double intervalHours)
        {
            List<DateTime> result = new List<DateTime>();
            for (int i = 0; i < (24 / intervalHours); i++)
            {
                DateTime time = new DateTime(1, 1, 1, 0, 0, 0);
                result.Add(time.AddHours(i * intervalHours));
            }
            return result;
        }
    }
}
