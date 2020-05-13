google.charts.load('current', { 'packages': ['treemap'] });
google.charts.setOnLoadCallback(go);

function numberWithCommas(x) {
    var parts = x.toString().split(".");
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return parts.join(".");
}

function dataRecieved(data) {
    google.charts.load('current', { 'packages': ['treemap'] });
    google.charts.setOnLoadCallback(drawChart(data));
}


function drawChart(json_data) {
    var data = google.visualization.arrayToDataTable(json_data);

    var options = {
        highlightOnMouseOver: true,
        maxDepth: 1,
        maxPostDepth: 2,
        minHighlightColor: '#8c6bb1',
        midHighlightColor: '#9ebcda',
        maxHighlightColor: '#edf8fb',
        minColor: '#009688',
        midColor: '#f7f7f7',
        maxColor: '#ee8100',
        headerHeight: 15,
        showScale: true,
        height: 500,
        useWeightedAverageForAggregation: true,
        generateTooltip: showFullTooltip
    };

    function showFullTooltip(row, size, value) {
        var prettySize = numberWithCommas(data.getValue(row, 0));
        return "<div style='background-color: #fff; padding: 4px;'><div><strong>" + prettySize + "</strong></div>" + "<div>" + size + " mb</div></div>";
    }

    tree = new google.visualization.TreeMap(document.getElementById('chart_div'));

    tree.draw(data, options);
}

function go() {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            var data = JSON.parse(xhr.responseText);
            dataRecieved(data);
        }
    };
    xhr.open("GET", "data.json");
    xhr.send();
}