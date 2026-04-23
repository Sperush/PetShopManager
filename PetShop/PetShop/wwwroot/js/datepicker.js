window.datepicker = {
    init: function (elementId, enableTime) {

        flatpickr.l10ns.vn.months.longhand = [
            "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
            "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"
        ];
        flatpickr.l10ns.vn.months.shorthand = [
            "Th1", "Th2", "Th3", "Th4", "Th5", "Th6",
            "Th7", "Th8", "Th9", "Th10", "Th11", "Th12"
        ];

        flatpickr("#" + elementId, {
            locale: "vn",
            enableTime: enableTime,
            dateFormat: enableTime ? "Y-m-d H:i" : "Y-m-d",
            altInput: true,
            altFormat: enableTime ? "d/m/Y h:i K" : "d/m/Y",
            time_24hr: false,
            allowInput: true,

            onInput: function (selectedDates, dateStr, instance) {

                if (instance.amPM && selectedDates.length > 0) {
                    instance.redraw();
                }
            },
            onClose: function (selectedDates, dateStr, instance) {

                var element = document.getElementById(elementId);
                if (element) {
                    var event = new Event('change', { bubbles: true });
                    element.dispatchEvent(event);
                }
            }
        });
    }
};

