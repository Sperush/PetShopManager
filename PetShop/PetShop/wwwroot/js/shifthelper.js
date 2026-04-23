window.shiftHelper = {
    registerCloseHandler: (employeeId) => {
        window.addEventListener('beforeunload', function (e) {
            const url = `/api/workshifts/close/${employeeId}`;
            navigator.sendBeacon(url);
        });
    },
    printQR: (imgUrl, secret) => {
        const printWindow = window.open('', '_blank');
        printWindow.document.write(`
            <html>
                <head>
                    <title>In mã QR Điểm Danh</title>
                    <style>
                        body { text-align: center; font-family: Arial, sans-serif; padding: 50px; }
                        .container { border: 2px dashed #000; padding: 30px; display: inline-block; border-radius: 15px; }
                        h2 { margin-top: 0; color: #333; }
                        img { width: 300px; height: 300px; margin: 20px 0; }
                        .footer { font-size: 14px; color: #666; }
                        .secret { font-weight: bold; background: #eee; padding: 5px 10px; border-radius: 5px; }
                    </style>
                </head>
                <body>
                    <div class="container">
                        <h2>MÃ QUÉT ĐIỂM DANH - PET SHOP</h2>
                        <p>Nhân viên vui lòng quét mã này để vào ca</p>
                        <img src="${imgUrl}" />
                        <div class="secret">Mã: ${secret}</div>
                        <p class="footer">In ngày: ${new Date().toLocaleString('vi-VN')}</p>
                    </div>
                    <script>
                        window.onload = () => {
                            window.print();
                            setTimeout(() => { window.close(); }, 500);
                        };
                    </script>
                </body>
            </html>
        `);
        printWindow.document.close();
    }
};

