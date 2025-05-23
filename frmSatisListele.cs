using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace StokTakipOtomasyonu
{
    public partial class frmSatisListele : Form
    {
        public frmSatisListele()
        {
            InitializeComponent();
        }

        MySqlConnection baglan = new MySqlConnection("Server=localhost;Database=stok_takip;Uid=root;Pwd=root;");
        DataSet ds = new DataSet();

        private void frmSatisListele_Load(object sender, EventArgs e)
        {
            SatisListele();
        }

        private void SatisListele()
        {
            baglan.Open();
            MySqlDataAdapter adtr = new MySqlDataAdapter("SELECT * FROM satislar", baglan);
            adtr.Fill(ds, "satislar");
            dataGridView1.DataSource = ds.Tables["satislar"];
            baglan.Close();
        }
    }
}
