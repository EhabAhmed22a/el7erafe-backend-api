using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Models;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Shared.DataTransferObject.ClientDTOs;

namespace Service.Helpers
{
    public static class AvailabilityLogic
    {
        public static bool IsTechnicianAvailable(Technician t, GetAvailableTechniciansRequest request)
        {
            var requestedDay = HelperClass.MapToWeekDay(request.Day.DayOfWeek);
            var schedules = t.Availability
                .Where(a =>
                    a.DayOfWeek == null ||
                    a.DayOfWeek == requestedDay)
                .ToList();

            if (!schedules.Any())
                return false;

            var reservations = t.Offers
                .Where(o =>
                    o.Reservation != null &&
                    (o.Reservation.Status == ReservationStatus.Confirmed ||
                     o.Reservation.Status == ReservationStatus.InProgress ||
                     o.Reservation.Status == ReservationStatus.InPayment) &&
                    o.ServiceRequest != null &&
                    o.ServiceRequest.ServiceDate == request.Day)
                .Select(o => o.Reservation)
                .OrderBy(r => r.Offer.WorkFrom ?? TimeOnly.MinValue)
                .ToList();

            foreach (var schedule in schedules)
            {
                // 🔹 Convert schedule to TimeSpan
                var workStartTs = NormalizeTime(schedule.FromTime);
                var workEndTs = NormalizeTime(schedule.ToTime);

                TimeSpan startTs;
                TimeSpan endTs;

                if (request.AllDayAvailable)
                {
                    startTs = workStartTs;
                    endTs = workEndTs;

                    // If request is for today, technician must be available after "Now"
                    if (request.Day == DateOnly.FromDateTime(HelperClass.GetEgyptNow()))
                    {
                        var nowTs = HelperClass.GetEgyptNow().TimeOfDay;
                        if (nowTs > startTs)
                            startTs = nowTs;

                        // If the tech's shift already ended or has less than 1 hour remaining from now
                        if (startTs >= endTs || (endTs - startTs) < TimeSpan.FromHours(1))
                            continue;
                    }
                }
                else
                {
                    var reqStartTs = NormalizeTime(request.FromTime!.Value);
                    var reqEndTs = NormalizeTime(request.ToTime!.Value);

                    startTs = workStartTs > reqStartTs ? workStartTs : reqStartTs;
                    endTs = workEndTs < reqEndTs ? workEndTs : reqEndTs;

                    // If request is for today
                    if (request.Day == DateOnly.FromDateTime(HelperClass.GetEgyptNow()))
                    {
                        var nowTs = HelperClass.GetEgyptNow().TimeOfDay;
                        if (nowTs > startTs)
                            startTs = nowTs;
                    }

                    // ❗ no overlap
                    if (startTs >= endTs)
                        continue;

                    // ❗ less than 1 hour
                    if ((endTs - startTs) < TimeSpan.FromHours(1))
                        continue;
                }

                if (HasFreeSlot(startTs, endTs, reservations))
                    return true;
            }

            return false;
        }
        public static bool HasFreeSlot(TimeSpan start, TimeSpan end, List<Reservation> reservations)
        {
            var current = start;

            foreach (var r in reservations)
            {
                if (r.Offer.WorkFrom == null || r.Offer.WorkTo == null)
                    continue;

                var resStart = NormalizeTime(r.Offer.WorkFrom.Value);
                var resEnd = NormalizeTime(r.Offer.WorkTo.Value);

                // clamp inside range
                if (resStart < start) resStart = start;
                if (resEnd > end) resEnd = end;

                if (resStart > current)
                {
                    var gap = resStart - current;
                    if (gap >= TimeSpan.FromHours(1))
                        return true;
                }

                if (resEnd > current)
                    current = resEnd;
            }

            var finalGap = end - current;
            if (finalGap >= TimeSpan.FromHours(1))
                return true;

            return false;
        }
        public static bool IsTechnicianAvailable(Technician t, DateOnly date, WeekDay requestedDay, TimeOnly? requestedFrom, TimeOnly? requestedTo)
        {
            var schedules = t.Availability
                .Where(a => a.DayOfWeek == null || a.DayOfWeek == requestedDay)
                .ToList();

            if (!schedules.Any()) return false;

            var reservations = t.Offers
                .Where(o =>
                    o.Reservation != null &&
                    (o.Reservation.Status == ReservationStatus.Confirmed ||
                     o.Reservation.Status == ReservationStatus.InProgress ||
                     o.Reservation.Status == ReservationStatus.InPayment) &&
                    o.ServiceRequest != null &&
                    o.ServiceRequest.ServiceDate == date)
                .Select(o => o.Reservation!)
                .OrderBy(r => r.Offer.WorkFrom ?? TimeOnly.MinValue)
                .ToList();

            bool isAllDay = !requestedFrom.HasValue || !requestedTo.HasValue;

            foreach (var schedule in schedules)
            {
                var workStartTs = NormalizeTime(schedule.FromTime);
                var workEndTs = NormalizeTime(schedule.ToTime);

                TimeSpan startTs;
                TimeSpan endTs;

                if (isAllDay)
                {
                    startTs = workStartTs;
                    endTs = workEndTs;

                    if (date == DateOnly.FromDateTime(HelperClass.GetEgyptNow()))
                    {
                        var nowTs = HelperClass.GetEgyptNow().TimeOfDay;
                        if (nowTs > startTs) startTs = nowTs;

                        if (startTs >= endTs || (endTs - startTs) < TimeSpan.FromHours(1))
                            continue;
                    }
                }
                else
                {
                    var reqStartTs = NormalizeTime(requestedFrom!.Value);
                    var reqEndTs = NormalizeTime(requestedTo!.Value);

                    startTs = workStartTs > reqStartTs ? workStartTs : reqStartTs;
                    endTs = workEndTs < reqEndTs ? workEndTs : reqEndTs;

                    if (date == DateOnly.FromDateTime(HelperClass.GetEgyptNow()))
                    {
                        var nowTs = HelperClass.GetEgyptNow().TimeOfDay;
                        if (nowTs > startTs) startTs = nowTs;
                    }

                    if (startTs >= endTs) continue;
                    if ((endTs - startTs) < TimeSpan.FromHours(1)) continue;
                }

                if (HasFreeSlot(startTs, endTs, reservations))
                    return true;
            }

            return false;
        }
        public static TimeSpan NormalizeTime(TimeOnly time)
        {
            // treat 23:59 as end of day (24:00)
            if (time == new TimeOnly(23, 59))
                return TimeSpan.FromHours(24);

            return time.ToTimeSpan();
        }
    }
}
