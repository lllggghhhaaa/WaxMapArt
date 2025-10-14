export function initSelect(element, dotNetRef) {
    const handleClickOutside = (event) => {
        if (element && !element.contains(event.target)) {
            dotNetRef.invokeMethodAsync('CloseFromOutside');
        }
    };

    document.addEventListener('click', handleClickOutside);

    element._cleanup = () => {
        document.removeEventListener('click', handleClickOutside);
    };
}