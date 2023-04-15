//https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-7.0#location-of-javascipt-1

export function beforeStart(options, extensions) {
}

export function afterStarted(blazor) {
    downloadFileJS()
}

export function scrollToBottom() {
    window.scrollTo(0, document.body.scrollHeight);
}

function downloadFileJS() {
    // working for JS file download
    window.downloadFileFromStream = async (fileName, contentStreamReference) => {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName ?? '';
        anchorElement.target = '_blank';
        anchorElement.click();
        anchorElement.remove();
        URL.revokeObjectURL(url);
    }
}