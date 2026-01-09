import { useEffect, useState } from 'react';
import api from "./api/axios"

function Tasks() {
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [newTaskTitle, setNewTaskTitle] = useState('');
  const [creating, setCreating] = useState(false);
  const [validationError, setValidationError] = useState('');

  // Fetch tasks on component mount
  useEffect(() => {
    fetchTasks();
  }, []);

  const fetchTasks = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await api.get('/tasks');
      setTasks(response.data);
    } catch (err) {
      console.error('Error fetching tasks:', err);
      setError('Failed to load tasks. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateTask = async (e) => {
    e.preventDefault();

    // Client-side validation
    if (!newTaskTitle.trim()) {
      setValidationError('Task title cannot be empty');
      return;
    }

    if (newTaskTitle.length > 200) {
      setValidationError('Task title must be less than 200 characters');
      return;
    }

    try {
      setCreating(true);
      setValidationError('');
      const response = await api.post('/tasks', { title: newTaskTitle });
      setTasks([...tasks, response.data]);
      setNewTaskTitle(''); // Clear input
    } catch (err) {
      console.error('Error creating task:', err);
      setValidationError(err.response?.data?.title?.[0] || 'Failed to create task');
    } finally {
      setCreating(false);
    }
  };

  const handleToggleTask = async (task) => {
    try {
      const response = await api.put(`/tasks/${task.id}`, {
        title: task.title,
        isDone: !task.isDone
      });
      setTasks(tasks.map(t => t.id === task.id ? response.data : t));
    } catch (err) {
      console.error('Error updating task:', err);
      alert('Failed to update task');
    }
  };

  const handleDeleteTask = async (taskId) => {
    if (!window.confirm('Are you sure you want to delete this task?')) {
      return;
    }

    try {
      await api.delete(`/tasks/${taskId}`);
      setTasks(tasks.filter(t => t.id !== taskId));
    } catch (err) {
      console.error('Error deleting task:', err);
      alert('Failed to delete task');
    }
  };

  if (loading) {
    return (
      <div>
        <h2>Tasks</h2>
        <p>Loading tasks...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div>
        <h2>Tasks</h2>
        <p style={{ color: 'red' }}>{error}</p>
        <button onClick={fetchTasks}>Retry</button>
      </div>
    );
  }

  return (
    <div>
      <h2>Tasks</h2>

      {/* Create Task Form */}
      <form onSubmit={handleCreateTask} style={{ marginBottom: '20px' }}>
        <input
          type="text"
          value={newTaskTitle}
          onChange={(e) => setNewTaskTitle(e.target.value)}
          placeholder="Enter new task..."
          disabled={creating}
          style={{ padding: '8px', marginRight: '10px', width: '300px' }}
        />
        <button type="submit" disabled={creating}>
          {creating ? 'Adding...' : 'Add Task'}
        </button>
        {validationError && (
          <p style={{ color: 'red', fontSize: '14px' }}>{validationError}</p>
        )}
      </form>

      {/* Task List */}
      {tasks.length === 0 ? (
        <p>No tasks yet. Create one above!</p>
      ) : (
        <ul style={{ listStyle: 'none', padding: 0 }}>
          {tasks.map(task => (
            <li key={task.id} style={{
              marginBottom: '10px',
              padding: '10px',
              border: '1px solid #ddd',
              borderRadius: '4px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between'
            }}>
              <span
                onClick={() => handleToggleTask(task)}
                style={{
                  cursor: 'pointer',
                  textDecoration: task.isDone ? 'line-through' : 'none',
                  flex: 1
                }}
              >
                {task.isDone ? '✅' : '❌'} {task.title}
              </span>
              <button
                onClick={() => handleDeleteTask(task.id)}
                style={{
                  background: '#ff4444',
                  color: 'white',
                  border: 'none',
                  padding: '5px 10px',
                  borderRadius: '4px',
                  cursor: 'pointer'
                }}
              >
                Delete
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default Tasks;
