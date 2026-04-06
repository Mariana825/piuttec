using Microsoft.EntityFrameworkCore;
using piuttec.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Grupo> Grupos { get; set; }
    public DbSet<Materia> Materias { get; set; }
    public DbSet<AlumnoGrupo> AlumnoGrupos { get; set; }
    public DbSet<DocenteMateria> DocenteMaterias { get; set; }
    public DbSet<Calificacion> Calificaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().ToTable("Usuario");
        modelBuilder.Entity<Grupo>().ToTable("Grupo");
        modelBuilder.Entity<Materia>().ToTable("Materia");
        modelBuilder.Entity<AlumnoGrupo>().ToTable("AlumnoGrupo");
        modelBuilder.Entity<DocenteMateria>().ToTable("DocenteMateria");
        modelBuilder.Entity<Calificacion>().ToTable("Calificacion");

        modelBuilder.Entity<AlumnoGrupo>()
            .HasKey(ag => new { ag.AlumnoId, ag.GrupoId });

        modelBuilder.Entity<DocenteMateria>()
            .HasKey(dm => new { dm.DocenteId, dm.MateriaId });
    }
}