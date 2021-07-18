using Microsoft.EntityFrameworkCore.Migrations;

namespace AutoMapperBlog.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Salary = table.Column<decimal>(type: "TEXT", nullable: false),
                    DepartmentId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employee_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Department",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Sales" });

            migrationBuilder.InsertData(
                table: "Department",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "Marketing" });

            migrationBuilder.InsertData(
                table: "Department",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "IT" });

            migrationBuilder.InsertData(
                table: "Employee",
                columns: new[] { "Id", "DepartmentId", "FirstName", "LastName", "Salary" },
                values: new object[] { 1, 1, "John", "Smith", 35000m });

            migrationBuilder.InsertData(
                table: "Employee",
                columns: new[] { "Id", "DepartmentId", "FirstName", "LastName", "Salary" },
                values: new object[] { 2, 1, "Mary", "Jones", 42000m });

            migrationBuilder.InsertData(
                table: "Employee",
                columns: new[] { "Id", "DepartmentId", "FirstName", "LastName", "Salary" },
                values: new object[] { 3, 1, "Mel", "Black", 25000m });

            migrationBuilder.InsertData(
                table: "Employee",
                columns: new[] { "Id", "DepartmentId", "FirstName", "LastName", "Salary" },
                values: new object[] { 4, 2, "Jack", "White", 55000m });

            migrationBuilder.InsertData(
                table: "Employee",
                columns: new[] { "Id", "DepartmentId", "FirstName", "LastName", "Salary" },
                values: new object[] { 5, 2, "Jess", "Tan", 47000m });

            migrationBuilder.InsertData(
                table: "Employee",
                columns: new[] { "Id", "DepartmentId", "FirstName", "LastName", "Salary" },
                values: new object[] { 6, 2, "Steve", "Davies", 22000m });

            migrationBuilder.InsertData(
                table: "Employee",
                columns: new[] { "Id", "DepartmentId", "FirstName", "LastName", "Salary" },
                values: new object[] { 7, 3, "Emma", "Williams", 47500m });

            migrationBuilder.InsertData(
                table: "Employee",
                columns: new[] { "Id", "DepartmentId", "FirstName", "LastName", "Salary" },
                values: new object[] { 8, 3, "Darren", "Miller", 28000m });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_DepartmentId",
                table: "Employee",
                column: "DepartmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Department");
        }
    }
}
