window.getBoundingClientRect = (element) => {
    if (!element) return null;
    const rect = element.getBoundingClientRect();
    return {
        Top: rect.top,
        Bottom: rect.bottom,
        Left: rect.left,
        Right: rect.right,
        Width: rect.width,
        Height: rect.height
    };
};
