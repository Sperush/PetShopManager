window.qrScanner = {
    instance: null,
};

window.startQRScanner = async (elementId, dotNetHelper) => {
    const config = { fps: 10, qrbox: { width: 250, height: 250 } };
    window.qrScanner.instance = new Html5QrcodeScanner(elementId, config, false);
    window.qrScanner.instance.render((decodedText) => {
        dotNetHelper.invokeMethodAsync('OnQRCodeScanned', decodedText);
        window.qrScanner.instance.clear();
    }, (error) => { });
};

window.stopQRScanner = () => {
    if (window.qrScanner.instance) {
        window.qrScanner.instance.clear();
    }
};

