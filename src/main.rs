use crossterm::{cursor, execute, terminal};
use std::{io::stdout, time::Duration, usize};

struct Vec3 {
    x: f32,
    y: f32,
    z: f32,
}

fn main() {
    let verts = vec![
        Vec3 {
            x: 1.0,
            y: 1.0,
            z: -1.0,
        },
        Vec3 {
            x: -1.0,
            y: 1.0,
            z: -1.0,
        },
        Vec3 {
            x: -1.0,
            y: -1.0,
            z: -1.0,
        },
        Vec3 {
            x: 1.0,
            y: -1.0,
            z: -1.0,
        },
        Vec3 {
            x: 1.0,
            y: -1.0,
            z: 1.0,
        },
        Vec3 {
            x: 1.0,
            y: 1.0,
            z: 1.0,
        },
        Vec3 {
            x: -1.0,
            y: 1.0,
            z: 1.0,
        },
        Vec3 {
            x: -1.0,
            y: -1.0,
            z: 1.0,
        },
    ];

    let mut edges: Vec<(usize, usize)> = vec![
        (0, 1),
        (1, 2),
        (2, 3),
        (3, 0),
        (4, 5),
        (5, 6),
        (6, 7),
        (7, 4),
        (0, 5),
        (1, 6),
        (2, 7),
        (3, 4),
    ];

    let (width, height) = terminal::size().unwrap_or((80, 24));

    let mut buffer: Vec<Vec<char>> = vec![vec!['x'; width as usize]; height as usize];

    let mut total_angle: f32 = 0.0;

    loop {
        for i in &mut buffer {
            for j in i {
                *j = ' ';
            }
        }

        // store projected 2d coordinates for each vertex
        let mut projected_points: Vec<(isize, isize)> = Vec::with_capacity(verts.len());

        for v in verts.iter() {
            let rotated_y = rotate_y(v, total_angle);
            let rotated_point = rotate_z(&rotated_y, total_angle);
            let projected = project(&rotated_point);
            if let Ok((x, y)) = convert(projected.0, projected.1) {
                projected_points.push((x as isize, y as isize));
            } else {
                projected_points.push((-1, -1)); // mark as invalid
            }
        }

        // draw edges
        for &(start, end) in &edges {
            let (x0, y0) = projected_points[start];
            let (x1, y1) = projected_points[end];
            if x0 >= 0 && y0 >= 0 && x1 >= 0 && y1 >= 0 {
                draw_line(x0, y0, x1, y1, &mut buffer);
            }
        }

		// do this here otherwise flickering
        execute!(stdout(), cursor::MoveTo(0, 0)).unwrap();

        let mut output = String::new();

        for row in &buffer {
            output.push_str(&row.iter().collect::<String>());
            output.push_str("\n");
        }

        output.pop();

        print!("{output}");
		
		// 16ms for 60fps
        std::thread::sleep(Duration::from_millis(16));
        total_angle += 0.05;
    }
}

fn rotate_z(vertex: &Vec3, angle: f32) -> Vec3 {
    let new_x = vertex.x * angle.cos() - vertex.y * angle.sin();
    let new_y = vertex.x * angle.sin() + vertex.y * angle.cos();
    Vec3 {
        x: new_x,
        y: new_y,
        z: vertex.z,
    }
}

fn rotate_y(vertex: &Vec3, angle: f32) -> Vec3 {
    let new_x = vertex.x * angle.cos() - vertex.z * angle.sin();
    let new_z = vertex.x * angle.sin() + vertex.z * angle.cos();
    Vec3 {
        x: new_x,
        y: vertex.y,
        z: new_z,
    }
}

// turns vec3 (x, y, z) into vec2 (u, v)
fn project(vertex: &Vec3) -> (f32, f32) {
    let z_calc = vertex.z + 5.0;

    let u = vertex.x / z_calc;
    let v = (vertex.y / z_calc) * 0.5;
    (u, v)
}


// convert two dimensional vertices into terminal coordinates
fn convert(mut x: f32, mut y: f32) -> Result<(usize, usize), String> {
    x *= 80.0;
    y *= 80.0;

    let (width, height) = terminal::size().unwrap_or((80, 24));

    x += (width as f32) / 2.0;
    y += (height as f32) / 2.0;

    if x < width as f32 && y < height as f32 && x > 0.0 && y > 0.0 {
        return Ok((x as usize, y as usize));
    } else {
        return Err(String::from("terminal cant fit vertices"));
    }
}

// my goat bresenham
fn draw_line(
    mut x0: isize,
    mut y0: isize,
    x1: isize,
    y1: isize,
    buffer: &mut Vec<Vec<char>>,
) {
    let dx = (x1 - x0).abs();
    let dy = (y1 - y0).abs();
    let sx = if x0 < x1 { 1 } else { -1 };
    let sy = if y0 < y1 { 1 } else { -1 };
    let mut err = dx - dy;
    let (width, height) = (buffer[0].len() as isize, buffer.len() as isize);

    loop {
        if x0 >= 0 && y0 >= 0 && x0 < width && y0 < height {
            buffer[y0 as usize][x0 as usize] = '#';
        }
        if x0 == x1 && y0 == y1 {
            break;
        }
        let e2 = 2 * err;
        if e2 > -dy {
            err -= dy;
            x0 += sx;
        }
        if e2 < dx {
            err += dx;
            y0 += sy;
        }
    }
}
