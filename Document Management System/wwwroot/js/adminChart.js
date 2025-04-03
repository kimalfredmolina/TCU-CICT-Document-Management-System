// Document Uploads Chart
const documentCtx = document.getElementById('documentChart').getContext('2d');
const documentChart = new Chart(documentCtx, {
    type: 'bar',
    data: {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],  // Example labels
        datasets: [{
            label: 'Documents Uploaded',
            data: [12, 19, 3, 5, 2, 3], // Example data
            backgroundColor: '#3498db',
            borderColor: '#2980b9',
            borderWidth: 1
        }]
    },
    options: {
        scales: {
            y: {
                beginAtZero: true
            }
        }
    }
});

// User Activity Chart
const activityCtx = document.getElementById('activityChart').getContext('2d');
const activityChart = new Chart(activityCtx, {
    type: 'line',
    data: {
        labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'], // Example labels
        datasets: [{
            label: 'User Activity',
            data: [5, 10, 8, 15, 12, 7, 9], // Example data
            borderColor: '#e74c3c',
            borderWidth: 2,
            fill: false
        }]
    },
    options: {
        scales: {
            y: {
                beginAtZero: true
            }
        }
    }
});
// Task Completion Status (Doughnut Chart)
const taskCompletionCtx = document.getElementById('taskCompletionChart').getContext('2d');
const taskCompletionChart = new Chart(taskCompletionCtx, {
    type: 'doughnut',
    data: {
        labels: ['Completed', 'Pending', 'Overdue'], // Example labels
        datasets: [{
            label: 'Task Completion Status',
            data: [60, 25, 15],  // Example data
            backgroundColor: ['#2ecc71', '#f39c12', '#e74c3c'],
            borderColor: '#fff',
            borderWidth: 2
        }]
    },
    options: {
        responsive: true,
        aspectRatio: 2,
        plugins: {
            legend: {
                position: 'top',
            },
            tooltip: {
                callbacks: {
                    label: function (tooltipItem) {
                        return tooltipItem.label + ': ' + tooltipItem.raw + '%';
                    }
                }
            }
        }
    }
});
