export function getScrollPosition(element) {
    const container = element;
    const scrollLeft = container.scrollLeft;
    const scrollWidth = container.scrollWidth;
    const clientWidth = container.clientWidth;

    const isAtStart = scrollLeft <= 5;
    const isAtEnd = scrollLeft >= scrollWidth - clientWidth - 5;

    if (scrollWidth <= clientWidth) {
        return "none";
    } else if (isAtStart) {
        return "left";
    } else if (isAtEnd) {
        return "right";
    } else {
        return "both";
    }
}

window.getBoundingClientRect = function(element) {
    return element.getBoundingClientRect();
};