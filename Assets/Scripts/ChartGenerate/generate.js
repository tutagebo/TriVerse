let chart = "";

for(let i = 0; i < 100; i++) {
    chart += JSON.stringify({
        lane: 2,
        group: 0,
        beat: i,
        type: "tap"
    })+",\n";
}
console.log(chart);